﻿// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable ForCanBeConvertedToForeach
namespace FluxUv
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Uv;

    internal static class ResponseWriter
    {
        private static readonly byte[] ColonSpace = Encoding.UTF8.GetBytes(": ");
        private static readonly byte[] CRLF = Encoding.UTF8.GetBytes("\r\n");
        public static int Write(FluxEnv env, out ArraySegment<byte> segment)
        {
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

            int headerLength = responseLineLength + 2 + MeasureHeaders(headers);

            var body = (Stream) env[OwinKeys.ResponseBody];

            int responseLength = headerLength + (int)body.Length;

            segment = BytePool.Intance.Get(responseLength);

            Encoding.UTF8.GetBytes(responseLine, 0, responseLineLength, segment.Array, segment.Offset);

            int offset = segment.Offset + responseLineLength;

            foreach (var header in headers)
            {
                if (header.Value.Length == 1)
                {
                    offset += Encoding.UTF8.GetBytes(header.Key, 0, header.Key.Length, segment.Array, offset);
                    ColonSpace.CopyTo(segment.Array, offset);
                    offset += 2;
                    string value = header.Value[0];
                    offset += Encoding.UTF8.GetBytes(value, 0, value.Length, segment.Array, offset);
                    CRLF.CopyTo(segment.Array, offset);
                    offset += 2;
                }
                else
                {
                    for (int i = 0; i < header.Value.Length; i++)
                    {
                        offset += Encoding.UTF8.GetBytes(header.Key, 0, header.Key.Length, segment.Array, offset);
                        ColonSpace.CopyTo(segment.Array, offset);
                        offset += 2;
                        string value = header.Value[i];
                        offset += Encoding.UTF8.GetBytes(value, 0, value.Length, segment.Array, offset);
                        CRLF.CopyTo(segment.Array, offset);
                        offset += 2;
                    }
                }
            }

            CRLF.CopyTo(segment.Array, offset);
            offset += 2;

            if (body.Length > 0)
            {
                body.Position = 0;
                body.Read(segment.Array, offset, (int)body.Length);
            }

            var bodyText = Encoding.UTF8.GetString(segment.Array, segment.Offset, responseLength);

            return responseLength;
        }

        private static int MeasureHeaders(IEnumerable<KeyValuePair<string, string[]>> headers)
        {
            int headerLength = 0;
            foreach (var header in headers)
            {
                if (header.Value.Length == 1)
                {
                    headerLength += header.Key.Length + header.Value[0].Length + 4;
                }
                else
                {
                    for (int i = 0; i < header.Value.Length; i++)
                    {
                        headerLength += header.Key.Length + header.Value[i].Length + 4;
                    }
                }
            }
            return headerLength;
        }
    }
}
// ReSharper restore ForCanBeConvertedToForeach
// ReSharper restore LoopCanBeConvertedToQuery