namespace FluxUv.Uv
{
    using System;
    using System.Runtime.InteropServices;

    internal class Http : IPoolObject
    {
        private static readonly Lib.AllocCallbackWin AllocCallback = AllocBuffer;
        private readonly Lib.ReadCallbackWin _uvReadCallback;
        private readonly Lib.Callback _uvWriteCallback;
        private IntPtr _client;
        private Action<Http, ArraySegment<byte>> _readCallback;
        private Action _writeCallback;
        private FluxEnv _env;

        public Http()
        {
            _uvReadCallback = ReadCallback;
            _uvWriteCallback = WriteCallback;
        }

        public FluxEnv Env
        {
            get { return _env; }
        }

        public void Reset()
        {
            _env = null;
            _readCallback = NullReadCallback;
            _writeCallback = NullWriteCallback;
        }

        public void Run(IntPtr client, Action<Http, ArraySegment<byte>> callback)
        {
            _client = client;
            _readCallback = callback;
            Lib.uv_read_start_win(_client, AllocCallback, _uvReadCallback);
        }

        private void ReadCallback(IntPtr stream, int size, WindowsBufferStruct buf)
        {
            var byteBuffer = BytePool.Intance.Get(size);
            Marshal.Copy(buf.@base, byteBuffer.Array, byteBuffer.Offset, size);
            Pointers.Free(buf.@base);
            _readCallback(this, byteBuffer);
        }

        public void FreeBuffer(ArraySegment<byte> buffer)
        {
            BytePool.Intance.Free(buffer);
        }

        public void Write(ArraySegment<byte> data, Action writeCallback)
        {
            _writeCallback = writeCallback ?? NullWriteCallback;
            int size = data.Count + 1;
            var responsePtr = Pointers.Alloc(size);
            Marshal.Copy(data.Array, data.Offset, responsePtr, data.Count);
            Marshal.WriteByte(responsePtr, data.Count, 0);
            var buf = Lib.uv_buf_init(responsePtr, (uint) size);
            var bufs = new[] {buf};
            var writeRequest = Pointers.Alloc(Lib.uv_req_size(RequestType.UV_WRITE));
            Lib.uv_write_win(writeRequest, _client, bufs, 1, _uvWriteCallback);
        }

        private void WriteCallback(IntPtr req, int status)
        {
            Pointers.Free(req);
            Lib.uv_close(_client, null);
            Pointers.Free(_client);
            _writeCallback();
        }

        private static WindowsBufferStruct AllocBuffer(IntPtr data, int size)
        {
            var ptr = Pointers.Alloc(size);
            var buf = Lib.uv_buf_init(ptr, (uint)size);
            return buf;
        }

        private static void NullReadCallback(Http arg1, ArraySegment<byte> arg2)
        {
        }

        private static void NullWriteCallback()
        {
        }

        public Http SetEnv(FluxEnv env)
        {
            _env = env;
            return this;
        }

        public void WriteEnv(Action writeCallback)
        {

        }
    }
}