namespace FluxUv
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ResponseLines
    {
        private static readonly Dictionary<object, byte[]> Lines;
        private static readonly Dictionary<object, string> Phrases;
        private const string Unknown = "Unknown";

        static ResponseLines()
        {
            Lines = Enumerable.Range(0, 600).ToDictionary(e => (object)e, e => Encoding.UTF8.GetBytes(string.Format("HTTP/1.1 {0} {1}\r\n", e, Unknown)));
            Phrases = Enumerable.Range(0, 600).ToDictionary(e => (object) e, _ => Unknown);
            SetPhrase(200, "OK");
        }

        static void SetPhrase(object key, string text)
        {
            Phrases[key] = text;
            Lines[key] = Encoding.UTF8.GetBytes(string.Format("HTTP/1.1 {0} {1}\r\n", key, text));
        }

        public static string GetPhrase(object key)
        {
            string phrase;
            return (Phrases.TryGetValue(key, out phrase)) ? phrase : Unknown;
        }

        public static byte[] GetLine(object key)
        {
            byte[] line;
            if (Lines.TryGetValue(key, out line)) return line;
            line = Encoding.UTF8.GetBytes(string.Format("HTTP/1.1 {0} Unknown\r\n", key));
            Lines[key] = line;
            return line;
        }
    }
}