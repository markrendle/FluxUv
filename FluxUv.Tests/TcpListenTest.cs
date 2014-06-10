namespace FluxUv.Tests
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Uv;
    using Xunit;

    public class TcpListenTest
    {
        private const int Port = 7001;

        [Fact]
        public void TcpServerCanListen()
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
                        Lib.uv_close(client, null);
                        Lib.uv_stop(loop);
                        stopped = true;
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

            SendString("Hello.");

            SpinWait.SpinUntil(() => stopped, 1000);

            if (!stopped)
            {
                Lib.uv_stop(loop);
            }
            cts.Cancel();

            Pointers.Free(client);
            Pointers.Free(server);

            Assert.Equal("Hello.", received);
        }

        private static void SendString(string str)
        {
            var ip = new IPAddress(new byte[] {127, 0, 0, 1});
            var bytes = Encoding.UTF8.GetBytes(str);
            using (var client = new TcpClient())
            {
                client.Connect(new IPEndPoint(IPAddress.Loopback, Port));
                client.GetStream().Write(bytes, 0, bytes.Length);
            }
        }

        private WindowsBufferStruct AllocBuffer(IntPtr data, int size)
        {
            var ptr = Pointers.Alloc(size);
            var buf = Lib.uv_buf_init(ptr, (uint)size);
            return buf;
        }
    }
}