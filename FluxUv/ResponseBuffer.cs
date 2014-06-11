namespace FluxUv
{
    using System;
    using System.Runtime.InteropServices;
    using Uv;

    public class ResponseBuffer
    {
        private IntPtr _ptr;
        private int _size;

        public WindowsBufferStruct[] Structs
        {
            get { return _structs; }
        }

        private readonly WindowsBufferStruct[] _structs = new WindowsBufferStruct[1];

        public int Write(ArraySegment<byte> data)
        {
            SetSize(data.Count + 1);
            Marshal.Copy(data.Array, data.Offset, _ptr, data.Count);
            _structs[0] = Lib.uv_buf_init(_ptr, (uint) data.Count + 1);
            return data.Count;
        }

        private void SetSize(int size)
        {
            if (size <= _size) return;

            Free();
            Alloc(size);
            _size = size;
        }

        private void Alloc(int size)
        {
            _ptr = Marshal.AllocHGlobal(size);
        }

        private void Free()
        {
            if (_ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_ptr);
            }
        }
    }
}