namespace FluxUv
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Uv;

    internal class Http : IPoolObject
    {
        private static readonly Lib.AllocCallbackWin AllocCallback = AllocBuffer;
        private static readonly ArraySegment<byte> EmptySegment = new ArraySegment<byte>(new byte[0]);
        private readonly Lib.ReadCallbackWin _uvReadCallback;
        private readonly Lib.Callback _uvWriteCallback;
        private readonly Action _uvCloseCallback;
        private readonly IntPtr _writeRequest = Marshal.AllocHGlobal(Lib.uv_req_size(RequestType.UV_WRITE));
        private readonly IntPtr _loop;
        private readonly IntPtr _server;
        private readonly IntPtr _client;
        private readonly Action<Http, ArraySegment<byte>> _readCallback;
        private readonly WindowsBufferStruct[] _bufferStructs = new WindowsBufferStruct[1];
        private Action<Http> _writeCallback;
        private readonly Action<Http> _closeCallback;
        private ArraySegment<byte> _writeSegment;
        private readonly FluxEnv _env = new FluxEnv();
        private readonly GCHandle _handle;
        private readonly ResponseBuffer _responseBuffer = new ResponseBuffer();

        public Http(IntPtr loop, IntPtr server, Action<Http, ArraySegment<byte>> readCallback, Action<Http> closeCallback)
        {
            _loop = loop;
            _server = server;
            _readCallback = readCallback;
            _closeCallback = closeCallback;
            _client = Marshal.AllocHGlobal(Lib.uv_handle_size(HandleType.UV_TCP));
            _uvReadCallback = ReadCallback;
            _uvWriteCallback = WriteCallback;
            _uvCloseCallback = CloseCallback;
            _handle = GCHandle.Alloc(this);
        }

        ~Http()
        {
            if (_handle.IsAllocated)
            {
                _handle.Free();
            }
        }

        public FluxEnv Env
        {
            get { return _env; }
        }

        public void Reset()
        {
            _env.Reset();
            _writeSegment = default(ArraySegment<byte>);
        }

        public void Run()
        {
            int error;

            if ((error = Lib.uv_tcp_init(_loop, _client)) == 0)
            {
                if ((error = Lib.uv_accept(_server, _client)) == 0)
                {
                    Lib.uv_read_start_win(_client, AllocCallback, _uvReadCallback);
                }
            }
            if (error != 0)
            {
                Lib.uv_close(_client, null);
            }
        }

        private void ReadCallback(IntPtr client, int size, WindowsBufferStruct buf)
        {
            if (size < 0)
            {
                Lib.uv_close(client, null);
                _readCallback(this, EmptySegment);
                return;
            }
            var byteBuffer = BytePool.Intance.Get(size);
            Marshal.Copy(buf.@base, byteBuffer.Array, byteBuffer.Offset, size);
            Pointers.Free(buf.@base);
            _readCallback(this, byteBuffer);
        }

        public void FreeBuffer(ArraySegment<byte> buffer)
        {
            BytePool.Intance.Free(buffer);
        }

        public void Write(ArraySegment<byte> data, Action<Http> writeCallback)
        {
            _writeCallback = writeCallback ?? NullWriteCallback;
            _writeSegment = data;
            _responseBuffer.Write(data);
            try
            {
                int error = Lib.uv_write_win(_writeRequest, _client, _responseBuffer.Structs, 1, _uvWriteCallback);

                if (error != 0)
                {
                    WriteCallback(_writeRequest, error);
                }
            }
            catch (AccessViolationException ex)
            {
                Trace.TraceError(ex.Message);
                throw;
            }
            
        }

        private void WriteCallback(IntPtr req, int status)
        {
            Lib.uv_close(_client, _uvCloseCallback);
            if (_writeSegment != default (ArraySegment<byte>))
            {
                BytePool.Intance.Free(_writeSegment);
            }
            _writeCallback(this);
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

        private static void NullWriteCallback(Http http)
        {
        }

        public void WriteEnv(Action<Http> writeCallback)
        {
            _writeSegment = ResponseWriter.Write(_env);
            Write(_writeSegment, writeCallback);
        }

        private void CloseCallback()
        {
            ClientPool.Free(_client);
            if (_closeCallback != null)
            {
                _closeCallback(this);
            }
        }
    }
}