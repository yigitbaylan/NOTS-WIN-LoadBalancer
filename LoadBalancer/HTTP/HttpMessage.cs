using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTTP
{
    public class HttpMessage
    {
        public string FirstLine;
        public List<HttpHeader> Headers;
        public byte[] Body;

        protected HttpMessage() { }

        protected HttpMessage(string firstLine, List<HttpHeader> headers, byte[] body)
        {
            FirstLine = firstLine;
            Headers = headers;
            Body = body;
        }

        public void AddHeader(string HeaderName, string HeaderValue)
        {
            Headers.Add(new HttpHeader(HeaderName, HeaderValue));
        }
        /// <summary>
        /// Update an existing header. Keep in mind that headers are case-insensitive according to RFC 7320 (https://tools.ietf.org/html/rfc7230#section-3.2).
        /// </summary>
        /// <param name="HeaderName">Name of the header that needs to be updated</param>
        /// <param name="HeaderValue">Value of the header that needs to be updated</param>
        public void UpdateHeader(string HeaderName, string HeaderValue)
        {
            var filter = Headers.Where(x => x.Name.ToLower() == HeaderName.ToLower());
            if (filter.Count() <= 0)
            {
                throw new KeyNotFoundException();
            }
            filter.ToArray()[0].Value = HeaderValue;
        }
        /// <summary>
        /// Delete an existing header. Keep in mind that headers are case-insensitive according to RFC 7320 (https://tools.ietf.org/html/rfc7230#section-3.2).
        /// </summary>
        /// <param name="HeaderName">Name of the header that needs to be deleted</param>
        public void DeleteHeader(string HeaderName)
        {
            var filter = Headers.Where(x => x.Name.ToLower() == HeaderName.ToLower());
            if (filter.Count() > 0)
            {
                Headers.Remove(filter.ToArray()[0]);
            }
        }
        /// <summary>
        /// Check if a header exist. Keep in mind that headers are case-insensitive according to RFC 7320 (https://tools.ietf.org/html/rfc7230#section-3.2).
        /// </summary>
        /// <param name="HeaderName">Header name to look for</param>
        /// <returns>Returns a boolean thats true if the header name exist. If not it will return a boolean that is false.</returns>
        public bool HasHeader(string HeaderName)
        {
            if (Headers.Where(x => x.Name.ToLower() == HeaderName.ToLower()).Count() > 0)
                return true;
            return false;
        }
        /// <summary>
        /// Get a single header. Keep in mind that headers are case-insensitive according to RFC 7320 (https://tools.ietf.org/html/rfc7230#section-3.2).
        /// </summary>
        /// <param name="HeaderName">Header name to look for</param>
        /// <returns>Returns a new HttpHeaderModel</returns>
        public HttpHeader GetHeader(string HeaderName)
        {
            var filter = Headers.Where(x => x.Name.ToLower() == HeaderName.ToLower());
            if (filter.Count() > 0)
            {
                return filter.ToArray()[0];
            }
            return null;
        }

        public string GetHeadersAsString()
        {
            string HeadersAsString = "";
            foreach (HttpHeader header in Headers)
            {
                HeadersAsString += header.ToString();
                HeadersAsString += "\r\n";
            }
            return HeadersAsString;
        }

        protected static List<string> ReadLines(byte[] ByteArray)
        {
            List<string> lines = new List<string>();
            StringBuilder sb = new StringBuilder();
            foreach (byte x in ByteArray)
            {
                if (x == '\n')
                {
                    string line = sb.ToString();
                    lines.Add(line);
                    sb.Clear();
                    continue;
                }
                if (x == '\r')
                    continue;
                sb.Append(Convert.ToChar(x));
            }
            lines.Add(sb.ToString());
            return lines;
        }

        protected static List<HttpHeader> ReadHeaders(List<string> lines, bool firstLineIncluded = true)
        {
            bool skipedFirst = false;
            List<HttpHeader> list = new List<HttpHeader>();
            foreach (string line in lines)
            {
                if (!skipedFirst && firstLineIncluded)
                {
                    skipedFirst = true;
                }
                else
                {
                    if (line.Equals(""))
                    {
                        break;
                    }
                    int Seperator = line.IndexOf(':');
                    if (Seperator == -1)
                    {
                        throw new Exception(string.Format("Invalid http header line: {0}", line));
                    }
                    string HeaderName = line.Substring(0, Seperator);
                    int Pos = Seperator + 1;
                    while ((Pos < line.Length) && (line[Pos] == ' '))
                    {
                        Pos++;
                    }
                    string HeaderValue = line.Substring(Pos, line.Length - Pos);
                    list.Add(new HttpHeader(HeaderName, HeaderValue));
                }
            }
            return list;
        }
        protected static byte[] ReadBody(byte[] ByteArray)
        {
            StringBuilder sb = new StringBuilder();
            int index = 0;
            for (int i = 0; i < ByteArray.Length; i++)
            {
                if (ByteArray[i] == '\n')
                {
                    string line = sb.ToString();
                    if (line == "")
                    {
                        index = i + 1;
                        break;
                    }
                    sb.Clear();
                    continue;
                }
                if (ByteArray[i] == '\r')
                    continue;
                sb.Append(Convert.ToChar(ByteArray[i]));
            }
            if (index > 0)
            {
                return ByteArray.Skip(index).ToArray();
            }
            return new byte[] { };
        }

        public override string ToString()
        {
            string headers = GetHeadersAsString();
            string body = GetBodyAsString();

            string message = $"{FirstLine}\r\n{headers}\r\n\r\n{body}";
            return message;
        }

        public string GetBodyAsString()
        {
            return Encoding.GetEncoding("ISO-8859-1").GetString(Body);
        }

        public byte[] ToByteArray()
        {
            List<byte> HttpMessage = new List<byte>();
            HttpMessage.AddRange(Encoding.ASCII.GetBytes(FirstLine));
            HttpMessage.AddRange(Encoding.ASCII.GetBytes(Environment.NewLine));
            HttpMessage.AddRange(Encoding.ASCII.GetBytes(GetHeadersAsString()));
            HttpMessage.AddRange(Encoding.ASCII.GetBytes(Environment.NewLine));
            HttpMessage.AddRange(Body);

            return HttpMessage.ToArray();
        }
    }
}
