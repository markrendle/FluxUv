// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable ForCanBeConvertedToForeach
namespace FluxUv
{
    using System.Collections.Generic;
    using System.Text;
    using Uv;

    internal static class ResponseWriter
    {
        public static void Write(Http http)
        {
            var env = http.Env;
            object statusCode;
            if (!env.TryGetValue(OwinKeys.ResponseStatusCode, out statusCode))
            {
                statusCode = 0;
            }
            object reasonPhrase;
            if (!env.TryGetValue(OwinKeys.ResponseReasonPhrase, out reasonPhrase))
            {
                reasonPhrase = "OK";
            }
            object responseProtocol;
            if (!env.TryGetValue(OwinKeys.ResponseProtocol, out responseProtocol))
            {
                responseProtocol = "HTTP/1.1";
            }

            var responseLine = string.Format("{0} {1} {2}\r\n", responseProtocol, statusCode, reasonPhrase);
            int responseLineLength = responseLine.Length;

            var headers = (IDictionary<string, string[]>) env[OwinKeys.ResponseHeaders];

            int headerLength = responseLineLength + 2;
            MeasureHeaders(headers, headerLength);

            var segment = BytePool.Intance.Get(headerLength);

            Encoding.UTF8.GetBytes(responseLine, 0, responseLineLength, segment.Array, segment.Offset);

            int offset = segment.Offset + responseLineLength;

            foreach (var header in headers)
            {
                if (header.Value.Length == 1)
                {
                    headerLength += header.Key.Length + header.Value[0].Length + 2;
                }
                else
                {
                    for (int i = 0; i < header.Value.Length; i++)
                    {
                        headerLength += header.Key.Length + header.Value[i].Length + 2;
                    }
                }
            }
        }

        private static void MeasureHeaders(IEnumerable<KeyValuePair<string, string[]>> headers, int headerLength)
        {
            foreach (var header in headers)
            {
                if (header.Value.Length == 1)
                {
                    headerLength += header.Key.Length + header.Value[0].Length + 2;
                }
                else
                {
                    for (int i = 0; i < header.Value.Length; i++)
                    {
                        headerLength += header.Key.Length + header.Value[i].Length + 2;
                    }
                }
            }
        }
    }
}
// ReSharper restore ForCanBeConvertedToForeach
// ReSharper restore LoopCanBeConvertedToQuery