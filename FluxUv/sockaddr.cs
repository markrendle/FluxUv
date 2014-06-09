namespace FluxUv
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct sockaddr
    {
        public short sin_family;
        public ushort sin_port;
    }
}