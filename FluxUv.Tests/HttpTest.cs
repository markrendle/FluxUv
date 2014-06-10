namespace FluxUv.Tests
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Uv;
    using Xunit;

    public class HttpTest
    {
        const string Body = "HTTP Hello!";
        private const string Newline = "\r\n";
        private const int Port = 7002;
        private string _result;
        bool _stopped = false;

        [Fact]
        public unsafe void SimpleHttpResponse()
        {
            IntPtr server;
            IntPtr client = IntPtr.Zero;
            string received = null;

            int listenStatus = 0;
            var cts = new CancellationTokenSource();
            var loop = Lib.uv_loop_new();
            server = Pointers.Alloc(Lib.uv_handle_size(HandleType.UV_TCP));
            Assert.Equal(0, Lib.uv_tcp_init(loop, server));
            var sockaddr = Lib.uv_ip4_addr(IPAddress.Loopback.ToString(), Port);
            Assert.Equal(0, Lib.uv_tcp_bind(server, sockaddr));
            Assert.Equal(0, Lib.uv_listen(server, 128, (srv, status) =>
            {
                if (status < 0)
                {
                    listenStatus = status;
                    return;
                }

                client = Pointers.Alloc(Lib.uv_handle_size(HandleType.UV_TCP));

                if (Lib.uv_tcp_init(loop, client) == 0 && Lib.uv_accept(server, client) == 0)
                {
                    var http = new Http();
                    http.Run(client, HttpCallback);
                }
                else
                {
                    Lib.uv_close(client, null);
                    Pointers.Free(client);
                    _stopped = true;
                }

            }));
            var runTask = Task.Run(() => Lib.uv_run(loop, uv_run_mode.UV_RUN_DEFAULT), cts.Token);

            SendHttpRequest();

            SpinWait.SpinUntil(() => _stopped, 1000);

            Lib.uv_stop(loop);
            cts.Cancel();

            Pointers.Free(server);

            Trace.WriteLine("Result: " + _result);
            Assert.Equal(Body, _result);
        }

        private void HttpCallback(Http http, ArraySegment<byte> arraySegment)
        {
            http.Write(new ArraySegment<byte>(Encoding.UTF8.GetBytes(CreateResponse())), WriteCallback);
        }

        private void WriteCallback()
        {
            _stopped = true;
        }

        private void SendHttpRequest()
        {
            using (var client = new HttpClient())
            {
                var task = client.GetStringAsync("http://127.0.0.1:" + Port + "/");
                Assert.DoesNotThrow(() =>
                {
                    task.Wait();
                    _result = task.Result;
                });
            }
        }

        private static string CreateResponse()
        {
            const string body = "HTTP Hello!";
            return @"HTTP/1.1 200 OK
Connection: close
Content-Type: text/plain
Content-Length: " + body.Length + @"

" + body;
        }
    }
}