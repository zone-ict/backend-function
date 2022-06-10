using System;
using System.Net.Http;

namespace Com.ZoneIct
{
    public static class HttpRequestMessageExtention
    {
        public static HttpRequestMessage Clone(this HttpRequestMessage req)
        {
            var clone = new HttpRequestMessage(req.Method, req.RequestUri);
            clone.Content = req.Content;
            clone.Version = req.Version;

            foreach (var opt in req.Options)
            {
                switch (opt.Value)
                {
                    case string s:
                        clone.Options.Set<string>(new HttpRequestOptionsKey<string>(opt.Key), s);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            foreach (var header in req.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            return clone;
        }
    }
}
