namespace FluxUv.Uv
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public struct sockaddr_in
    {
        public int a, b, c, d;
    }
}