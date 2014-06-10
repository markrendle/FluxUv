namespace FluxUv
{
    using System;
    using System.Net;
    using System.Runtime.Remoting.Channels;
    using System.Threading;
    using System.Threading.Tasks;
    using Uv;
    using AppFunc = System.Func< // Call
        System.Collections.Generic.IDictionary<string, object>, // Environment
                System.Threading.Tasks.Task>;
    public class FluxServer
    {
        private readonly IPAddress _ipAddress;
        private readonly int _port;
        private int _started;
        private int _stopped;
        private IntPtr _server;
        private IntPtr _loop;
        private readonly Lib.Callback _listenCallback;
        private readonly Action<Http, ArraySegment<byte>> _httpCallback;
        private readonly Action<Http> _writeCallback;
        private AppFunc _app;
        private Task _task;
        private readonly Pool<FluxEnv> _envPool = new Pool<FluxEnv>();
        private readonly Pool<Http> _httpPool = new Pool<Http>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource(1000);

        private Http _http;

        public FluxServer(int port) : this(IPAddress.Loopback, port)
        {
        }

        public FluxServer(IPAddress ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
            _listenCallback = ListenCallback;
            _httpCallback = HttpCallback;
            _writeCallback = WriteCallback;
        }

        public void Start(AppFunc app)
        {
            if (1 != Interlocked.Increment(ref _started)) throw new InvalidOperationException("Server is already started.");
            _app = app;

            _loop = Lib.uv_default_loop();
            _server = Pointers.Alloc(Lib.uv_handle_size(HandleType.UV_TCP));
            int error;
            if ((error = Lib.uv_tcp_init(_loop, _server)) != 0)
            {
                throw new FluxUvException("uv_tcp_init fail " + error);
            }

            var sockaddr = Lib.uv_ip4_addr(_ipAddress.ToString(), _port);
            if ((error = Lib.uv_tcp_bind(_server, sockaddr)) != 0)
            {
                throw new FluxUvException("uv_tcp_bind fail " + error);
            }

            if ((error = Lib.uv_listen(_server, 128, _listenCallback)) != 0)
            {
                throw new FluxUvException("uv_listen fail " + error);
            }

            _task = Task.Run(() => Lib.uv_run(_loop, uv_run_mode.UV_RUN_DEFAULT), _cts.Token);
        }

        public void Stop()
        {
            if (_started == 0 || 1 != Interlocked.Increment(ref _stopped)) throw new InvalidOperationException("Server is already stopped.");
            Lib.uv_stop(_loop);
            _cts.Cancel();
        }

        private void ListenCallback(IntPtr server, int status)
        {
            if (status != 0) return;

            var client = Pointers.Alloc(Lib.uv_handle_size(HandleType.UV_TCP));

            int error;

            if ((error = Lib.uv_tcp_init(_loop, client)) == 0)
            {
                if ((error = Lib.uv_accept(server, client)) == 0)
                {
                    var http = _httpPool.Pop();
                    http.Run(client, _httpCallback);
                }
            }
            if (error != 0)
            {
                Lib.uv_close(client, null);
                Pointers.Free(client);
            }
        }

        private void HttpCallback(Http http, ArraySegment<byte> data)
        {
            ByteRequestParser.Parse(data, http.Env);
            _app(http.Env).ContinueWith(AppCallback, http);
        }

        private void AppCallback(Task task, object state)
        {
            var http = (Http) state;
            if (task.IsFaulted)
            {
                http.Write(StockResponses.InternalServerError, _writeCallback);
            }
            else
            {
                http.WriteEnv(_writeCallback);
            }
        }

        private void WriteCallback(Http http)
        {
            _httpPool.Push(http);
        }
    }
}