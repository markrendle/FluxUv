namespace FluxUv
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ResponseLines
    {
        private static readonly Dictionary<object, byte[]> Lines;

        static ResponseLines()
        {
            Lines = Enumerable.Range(0, 600).ToDictionary(e => (object)e, e => Encoding.UTF8.GetBytes(string.Format("HTTP/1.1 {0} Unknown\r\n", e)));
            SetLine(200, "200 OK");
        }

        static void SetLine(object key, string text)
        {
            Lines[key] = Encoding.UTF8.GetBytes("HTTP/1.1 " + text + "\r\n");
        }

        public static byte[] Get(object key)
        {
            byte[] line;
            if (Lines.TryGetValue(key, out line)) return line;
            line = Encoding.UTF8.GetBytes(string.Format("HTTP/1.1 {0} Unknown\r\n", key));
            Lines[key] = line;
            return line;
        }
    }
}