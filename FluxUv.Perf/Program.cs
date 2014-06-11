using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluxUv.Perf
{
    using System.Globalization;
    using System.IO;

    class Program
    {
        private static readonly object OK = 200;
        private static readonly byte[] HtmlBytes;
        private static readonly string[] ContentType = { "text/html; charset=utf-8" };
        private static readonly string[] ContentLength;

        static Program()
        {
            HtmlBytes = Encoding.UTF8.GetBytes(Properties.Resources.FluxHtml);
            ContentLength = new[] {(HtmlBytes.Length + 1).ToString(CultureInfo.InvariantCulture)};
        }

        static void Main(string[] args)
        {
            var server = new FluxServer(3589);
            server.Start(App);
            Console.WriteLine(@"Flux listening on port 3589. Press Enter to stop...");
            Console.ReadLine();
            server.Stop();
        }

        private static Task App(IDictionary<string, object> env)
        {
            env[OwinKeys.ResponseStatusCode] = OK;
            var headers = (IDictionary<string, string[]>) env[OwinKeys.ResponseHeaders];
            headers["Content-Type"] = ContentType;

            headers["Content-Length"] = ContentLength;

            var responseStream = (Stream) env[OwinKeys.ResponseBody];
            responseStream.Write(HtmlBytes, 0, HtmlBytes.Length);
            return Task.FromResult<object>(null);
        }
    }
}
