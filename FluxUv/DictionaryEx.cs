namespace FluxUv
{
    using System;
    using System.Collections.Generic;

    static class DictionaryEx
    {
        public static void AddHeader(this IDictionary<string, string[]> headers, string key, string value)
        {
            string[] exist;
            if (headers.TryGetValue(key, out exist))
            {
                Array.Resize(ref exist, exist.Length + 1);
                exist[exist.Length - 1] = value;
            }
            else
            {
                exist = new[] { value };
            }
            headers[key] = exist;
        }
    }
}