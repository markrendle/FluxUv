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
    using Uv;
    using Xunit;

    public class TcpFixture
    {
        private const int Port = 7000;

        [Fact]
        public void CanGetDefaultLoop()
        {
            var loop = Lib.uv_default_loop();
            Assert.NotEqual(IntPtr.Zero, loop);
        }

        [Fact]
        public void CanCreateNewLoop()
        {
            var loop = Lib.uv_loop_new();
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
            var sockaddr = Lib.uv_ip4_addr("0.0.0.0", Port);
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

        private void ReadCallback(IntPtr stream, int size, WindowsBufferStruct buf)
        {

        }
    }
}
