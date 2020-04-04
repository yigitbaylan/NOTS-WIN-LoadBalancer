using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace HTTP
{
    public class HttpResponse : HttpMessage
    {
        public new string FirstLine
        {
            get => base.FirstLine;
            set
            {
                base.FirstLine = value;
            }
        }


        public HttpResponse() { }


        public HttpResponse(string firstline, List<HttpHeader> headers, byte[] body) : base(firstline, headers, body)
        {

        }

        public static HttpResponse Parse(byte[] request)
        {
            try
            {
                var lines = ReadLines(request);
                return new HttpResponse(lines[0], ReadHeaders(lines), ReadBody(request)); ;
            }
            catch
            {
                throw new FormatException();
            }
        }

        public string SetServerCookie(string server)
        {
            string cookie = "ConnectedServer=" + server + "; Path=/; Expires=" + DateTime.Now.AddMinutes(1).ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture.DateTimeFormat) + "; HttpOnly";
            if (HasHeader("Set-Cookie"))
            {
                DeleteHeader("Set-Cookie");
                Headers.Add(new HttpHeader("Set-Cookie", cookie));
            }
            else
                Headers.Add(new HttpHeader("Set-Cookie", cookie));

            return cookie;
        }


        public string GetSessionCookie()
        {
            string receivedCookie = "";

            if (HasHeader("Set-Cookie"))
                receivedCookie = GetHeader("Set-Cookie").Value;
            return receivedCookie == "" ? "NO_SESSION" : receivedCookie;
        }

        public static HttpResponse Get500Error()
        {
            string firstLine = "HTTP/1.1 500 Internal Server Error";
            byte[] body = File.ReadAllBytes("../../../../HTTP/Assets/HTML/500.html");
            List<HttpHeader> headers = new List<HttpHeader>();

            headers.Add(new HttpHeader("Content-Type", "text/html" + "\r\n"));

            HttpResponse httpMessage = new HttpResponse(firstLine, headers, body);

            return httpMessage;
        }
        public static HttpResponse Get503Error()
        {
            string firstLine = "HTTP/1.1 503 Service Unavailable";
            byte[] body = File.ReadAllBytes("../../../../HTTP/Assets/HTML/503.html");
            List<HttpHeader> headers = new List<HttpHeader>();

            headers.Add(new HttpHeader("Content-Type", "text/html" + "\r\n"));

            HttpResponse httpMessage = new HttpResponse(firstLine, headers, body);

            return httpMessage;
        }
    }
}
