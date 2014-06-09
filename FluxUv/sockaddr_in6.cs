namespace FluxUv
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Size = 28)]
    public struct sockaddr_in6
    {
        public int a, b, c, d, e, f, g;
    }
}