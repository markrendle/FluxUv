namespace FluxUv.Uv
{
    using System;
    using System.Runtime.InteropServices;

    public unsafe class Lib
    {
        [DllImport("uv")]
        public static extern IntPtr uv_default_loop();

        [DllImport("uv")]
        public static extern IntPtr uv_loop_new();

        [DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_tcp_init(IntPtr loop, IntPtr handle);

        [DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_handle_size(HandleType type);

        [DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_req_size(RequestType type);

        [DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
        public extern static sockaddr_in uv_ip4_addr(string ip, int port);

        [DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
        public extern static sockaddr_in6 uv_ip6_addr(string ip, int port);

        [DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_tcp_bind(IntPtr server, sockaddr_in sockaddr);

        [DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_listen(IntPtr stream, int backlog, Callback callback);

        [DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_run(IntPtr loop, uv_run_mode mode);

        [DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr uv_stop(IntPtr loop);

        [DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_accept(IntPtr server, IntPtr client);

        [DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void uv_close(IntPtr client, Action callback);

        [DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
        public static extern WindowsBufferStruct uv_buf_init(IntPtr @base, uint length);

        [DllImport("uv", EntryPoint = "uv_read_start", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_read_start_win(IntPtr stream, AllocCallbackWin allocCallback, ReadCallbackWin readCallback);

        [DllImport("uv", EntryPoint = "uv_write", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_write_win(IntPtr req, IntPtr stream, WindowsBufferStruct[] buffers, int bufferCount, Callback callback);
        [DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_timer_init(IntPtr loop, IntPtr timer);

        [DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_timer_start(IntPtr timer, Callback callback, ulong timeout, ulong repeat);

        [DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_timer_stop(IntPtr timer);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Callback(IntPtr req, int status);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate WindowsBufferStruct AllocCallbackWin(IntPtr data, int size);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ReadCallbackWin(IntPtr stream, int size, WindowsBufferStruct buf);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WindowsBufferStruct
    {
        internal WindowsBufferStruct(IntPtr @base, int length)
            : this(@base, (uint)length)
        {
        }

        internal WindowsBufferStruct(IntPtr @base, long length)
            : this(@base, (uint)length)
        {
        }

        internal WindowsBufferStruct(IntPtr @base, IntPtr length)
            : this(@base, length.ToInt64())
        {
        }

        internal WindowsBufferStruct(IntPtr @base, uint length)
        {
            this.@base = @base;
            this.length = length;
        }

        public uint length;
        public IntPtr @base;
    }
}
