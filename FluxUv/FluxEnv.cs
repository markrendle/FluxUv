namespace FluxUv
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

    internal class FluxEnv : IDictionary<string, object>, IPoolObject
    {
        private readonly IDictionary<string, object> _internal;
        private readonly IDictionary<string, string[]> _requestHeaders;
        private readonly IDictionary<string, string[]> _responseHeaders;
        private readonly MemoryStream _responseBody;

        public FluxEnv()
        {
            _internal = new Dictionary<string, object>(16);
            _internal[OwinKeys.Version] = "1.0";
            _internal[OwinKeys.RequestHeaders] = _requestHeaders = new Dictionary<string, string[]>(16);
            _internal[OwinKeys.ResponseHeaders] = _responseHeaders = new Dictionary<string, string[]>(16);
            _internal[OwinKeys.ResponseBody] = _responseBody = new MemoryStream(1024);
        }

        public void Reset()
        {
            _responseBody.SetLength(0);
            _requestHeaders.Clear();
            _responseHeaders.Clear();
            _internal.Remove(OwinKeys.ResponseStatusCode);
            _internal.Remove(OwinKeys.ResponseReasonPhrase);
            _internal.Remove(OwinKeys.ResponseProtocol);
            _internal.Remove(OwinKeys.RequestBody);
            _internal.Remove(OwinKeys.ResponseBody);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _internal.GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            _internal.Add(item);
        }

        public void Clear()
        {
            _internal.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _internal.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            _internal.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return _internal.Remove(item);
        }

        public int Count
        {
            get { return _internal.Count; }
        }

        public bool IsReadOnly
        {
            get { return _internal.IsReadOnly; }
        }

        public bool ContainsKey(string key)
        {
            return _internal.ContainsKey(key);
        }

        public void Add(string key, object value)
        {
            _internal.Add(key, value);
        }

        public bool Remove(string key)
        {
            return _internal.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _internal.TryGetValue(key, out value);
        }

        public object this[string key]
        {
            get { return _internal[key]; }
            set { _internal[key] = value; }
        }

        public ICollection<string> Keys
        {
            get { return _internal.Keys; }
        }

        public ICollection<object> Values
        {
            get { return _internal.Values; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}