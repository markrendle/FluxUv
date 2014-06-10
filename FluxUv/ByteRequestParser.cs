namespace FluxUv
{
    using System;
    using System.Collections.Generic;

    public static unsafe class ByteRequestParser
    {
        private const int Method = 0;
        private const int Path = 1;
        private const int QueryString = 2;
        private const int Protocol = 3;

        private const sbyte Space = (sbyte)' ';
        private const sbyte QuestionMark = (sbyte)'?';
        private const sbyte CarriageReturn = (sbyte)'\r';
        private const sbyte Newline = (sbyte)'\n';
        private const sbyte Colon = (sbyte)':';

        private static readonly string[] RequestSections =
        {
            OwinKeys.RequestMethod, OwinKeys.RequestPath, OwinKeys.RequestQueryString, OwinKeys.RequestProtocol
        };

        public static IDictionary<string, object> Parse(ArraySegment<byte> segment, IDictionary<string, object> dict)
        {
            int requestSection = 0;
            bool queryString = false;

            fixed (byte* b = segment.Array)
            {
                sbyte* end = ((sbyte*)b) + segment.Offset + segment.Count;
                sbyte* start = ((sbyte*)b) + segment.Offset;
                sbyte* c;
                for (c = start; c < end; c++)
                {
                    switch (*c)
                    {
                        case Space:
                            dict[RequestSections[requestSection]] = new string(start, 0, (int)(c - start));
                            start = ++c;
                            if (requestSection == Path)
                            {
                                requestSection += 2;
                            }
                            else
                            {
                                ++requestSection;
                            }
                            break;
                        case QuestionMark:
                            if (requestSection == Path)
                            {
                                queryString = true;
                                dict[RequestSections[requestSection]] = new string(start, 0, (int)(c - start));
                                start = ++c;
                                ++requestSection;
                            }
                            break;
                        case CarriageReturn:
                            dict[RequestSections[requestSection]] = new string(start, 0, (int)(c - start));
                            c++;
                            goto next;
                    }
                }
                next:
                if (!queryString)
                {
                    dict["owin.RequestQueryString"] = string.Empty;
                }
                dict["owin.RequestHeaders"] = ParseHeaders(++c, end);
            }
            return dict;
        }

        private static IDictionary<string, string[]> ParseHeaders(sbyte* start, sbyte* end)
        {
            var dict = new Dictionary<string, string[]>();
            bool inKey = true;
            for (var c = start; c < end; c++)
            {
                if (*c == CarriageReturn)
                {
                    break;
                }

                while (*++c != Colon) { }
                int keyLength = (int)(c - start);

                while (*++c == Space) { }
                sbyte* valueStart = c;

                while (*++c != CarriageReturn) { }
                int valueLength = (int)(c - valueStart);
                dict.AddHeader(new string(start, 0, keyLength), new string(valueStart, 0, valueLength));

                ++c;
                start = c + 1;
            }
            return dict;
        }
    }
}