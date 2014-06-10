namespace FluxUv.Uv
{
    using System;
    using System.Runtime.InteropServices;

    public static class Pointers
    {
        public static IntPtr Alloc(int size)
        {
            return Marshal.AllocHGlobal(size);
        }

        public static void Free(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }
}