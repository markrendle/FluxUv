namespace FluxUv.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    public class ServerTest
    {
        private const int Port = 7004;
        private bool _appCalled;
        private string _result;

        [Fact]
        public void OwinServerWorks()
        {
            var server = new FluxServer(Port);
            server.Start(App);
            try
            {
                SendHttpRequest();
            }
            finally
            {
                server.Stop();
            }
            Assert.True(_appCalled);
            Assert.Equal(Properties.Resources.ServerTestHtml, _result);
        }

        private Task App(IDictionary<string, object> env)
        {
            _appCalled = true;
            env[OwinKeys.ResponseStatusCode] = 200;
            env[OwinKeys.ResponseReasonPhrase] = "OK";
            var headers = (IDictionary<string, string[]>) env[OwinKeys.ResponseHeaders];
            headers["Content-Type"] = new[] {"text/html; charset=utf-8"};

            var htmlBytes = Encoding.UTF8.GetBytes(Properties.Resources.ServerTestHtml);

            headers["Content-Length"] = new[] {htmlBytes.Length.ToString(CultureInfo.InvariantCulture)};

            var responseStream = (Stream) env[OwinKeys.ResponseBody];
            responseStream.Write(htmlBytes, 0, htmlBytes.Length);
            return Task.FromResult<object>(null);
        }

        private void SendHttpRequest()
        {
            using (var client = new HttpClient())
            {
                var task = client.GetStringAsync("http://127.0.0.1:" + Port + "/");
                try
                {
                    task.Wait();
                    _result = task.Result;
                }
                catch (AggregateException ex)
                {
                    _result = null;
                    throw;
                }
            }
        }
    }
}