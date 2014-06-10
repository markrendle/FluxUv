namespace FluxUv.Tests
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Uv;
    using Xunit;

    public class RawHttpTest
    {
        const string Body = "HTTP Hello!";
        private const string Newline = "\r\n";
        private const int Port = 7002;
        private string _result;

        [Fact]
        public unsafe void SimpleHttpResponse()
        {
            IntPtr server;
            IntPtr client = IntPtr.Zero;
            string received = null;
            bool stopped = false;

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
                int error = Lib.uv_tcp_init(loop, client);

                error = Lib.uv_accept(server, client);
                if (error == 0)
                {
                    Lib.uv_read_start_win(client, AllocBuffer, (stream, size, buffer) =>
                    {
                        received = Marshal.PtrToStringAnsi(buffer.@base, size);
                        Pointers.Free(buffer.@base);
                        string response = CreateResponse();
                        var responsePtr = Marshal.StringToHGlobalAnsi(response);
                        var buf = Lib.uv_buf_init(responsePtr, (uint) response.Length + 1);
                        var bufs = new[] {buf};
                        var writeRequest = Pointers.Alloc(Lib.uv_req_size(RequestType.UV_WRITE));
                        Lib.uv_write_win(writeRequest, client, bufs, 1, (req, i) => {
                            Lib.uv_close(client, null);
                            Lib.uv_stop(loop);
                            stopped = true;
                        });
                    });
                }
                else
                {
                    Lib.uv_close(client, null);
                    Lib.uv_stop(loop);
                    stopped = true;
                }

            }));
            var runTask = Task.Run(() => Lib.uv_run(loop, uv_run_mode.UV_RUN_DEFAULT), cts.Token);

            SendHttpRequest();

            SpinWait.SpinUntil(() => stopped, 1000);

            if (!stopped)
            {
                Lib.uv_stop(loop);
            }
            cts.Cancel();

            Pointers.Free(client);
            Pointers.Free(server);

            Trace.WriteLine("Result: " + _result);
            Assert.Equal(Body, _result);
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

        private WindowsBufferStruct AllocBuffer(IntPtr data, int size)
        {
            var ptr = Pointers.Alloc(size);
            var buf = Lib.uv_buf_init(ptr, (uint)size);
            return buf;
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