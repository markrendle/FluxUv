using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluxUv
{
    using System.Runtime.InteropServices;

    public unsafe class Lib
    {
        [DllImport("uv")]
        public static extern IntPtr uv_default_loop();

        [DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_tcp_init(IntPtr loop, IntPtr handle);

        [DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_handle_size(HandleType type);

        [DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
        public extern static sockaddr_in uv_ip4_addr(string ip, int port);

        [DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
        public extern static sockaddr_in6 uv_ip6_addr(string ip, int port);

        [DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_tcp_bind(IntPtr server, sockaddr_in sockaddr);

        [DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_listen(IntPtr stream, int backlog, ListenCallback listenCallback);

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

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ListenCallback(IntPtr req, int status);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate WindowsBufferStruct AllocCallbackWin(IntPtr data, int size);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ReadCallbackWin(IntPtr stream, int size, WindowsBufferStruct buf);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WindowsBufferStruct
    {
        public uint length;
        public IntPtr @base;
    }
}
