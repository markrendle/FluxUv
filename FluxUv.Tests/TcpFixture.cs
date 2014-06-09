namespace FluxUv.Tests
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class TcpFixture
    {
        [Fact]
        public void CanCreateLoop()
        {
            var loop = Lib.uv_default_loop();
            Assert.NotEqual(IntPtr.Zero, loop);
        }

        [Fact]
        public void LoopCanRunOnce()
        {
            var loop = Lib.uv_default_loop();
            var error = Lib.uv_run(loop, uv_run_mode.UV_RUN_ONCE);
            Assert.Equal(0, error);
        }

        [Fact]
        public void CanCreateTcpServer()
        {
            var loop = Lib.uv_default_loop();
            var server = Pointers.Alloc(Lib.uv_handle_size(HandleType.UV_TCP));
            int error = -1;
            Assert.DoesNotThrow(() =>
                error = Lib.uv_tcp_init(loop, server));
            Assert.Equal(0, error);
            Pointers.Free(server);
        }

        [Fact]
        public void CanCreateSockAddrIn()
        {
            var sockaddr = Lib.uv_ip4_addr("0.0.0.0", 7000);
            Assert.NotEqual(0, sockaddr.a);
        }

        [Fact]
        public void CanCreateBuffer()
        {
            const int size = 1024;
            var ptr = Pointers.Alloc(size);
            var buf = Lib.uv_buf_init(ptr, size);
            Assert.Equal(size, (int)buf.length);
        }

        [Fact]
        public void TcpServerCanListen()
        {
            IntPtr server;
            IntPtr client = IntPtr.Zero;
            string received = null;
            bool stopped = false;

            int listenStatus = 0;
            var cts = new CancellationTokenSource();
            var loop = Lib.uv_default_loop();
            server = Pointers.Alloc(Lib.uv_handle_size(HandleType.UV_TCP));
            Assert.Equal(0, Lib.uv_tcp_init(loop, server));
            var sockaddr = Lib.uv_ip4_addr(IPAddress.Loopback.ToString(), 7000);
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
                client.Connect(new IPEndPoint(IPAddress.Loopback, 7000));
                client.GetStream().Write(bytes, 0, bytes.Length);
            }
        }

        private void ReadCallback(IntPtr stream, int size, WindowsBufferStruct buf)
        {

        }

        private WindowsBufferStruct AllocBuffer(IntPtr data, int size)
        {
            var ptr = Pointers.Alloc(size);
            return Lib.uv_buf_init(ptr, (uint)size);
        }
    }
}
