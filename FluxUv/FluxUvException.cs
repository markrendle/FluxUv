namespace FluxUv
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class FluxUvException : Exception
    {
        public FluxUvException()
        {
        }

        public FluxUvException(string message) : base(message)
        {
        }

        public FluxUvException(string message, Exception inner) : base(message, inner)
        {
        }

        protected FluxUvException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}