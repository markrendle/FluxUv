namespace FluxUv
{
    using System;
    using System.Text;

    internal static class StockResponses
    {
        public static ArraySegment<byte> InternalServerError = new ArraySegment<byte>(Encoding.UTF8.GetBytes("HTTP/1.1 500 Internal Server Error\r\nConnection: close\r\n\r\n"));
    }
}