namespace FluxUv
{
    using System;
    using System.Collections.Concurrent;
    using System.Runtime.InteropServices;
    using Uv;

    internal static class ClientPool
    {
        private static readonly ConcurrentStack<IntPtr> Pool = new ConcurrentStack<IntPtr>();

        public static IntPtr Get()
        {
            IntPtr ptr;
            if (Pool.TryPop(out ptr)) return ptr;
            return Marshal.AllocHGlobal(Lib.uv_handle_size(HandleType.UV_TCP));
        }

        public static void Free(IntPtr ptr)
        {
            Pool.Push(ptr);
        }
    }
}