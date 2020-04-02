using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace LoadBalancer.Models.HTTP
{
    class HttpResponseModel : HttpMessageModel
    {
        public new string FirstLine
        {
            get => base.FirstLine;
            set
            {
                base.FirstLine = value;
            }
        }


        public HttpResponseModel() { }


        public HttpResponseModel(string firstline, List<HttpHeaderModel> headers, byte[] body) : base(firstline, headers, body)
        {

        }

        public static HttpResponseModel Parse(byte[] request)
        {
            try
            {
                var lines = ReadLines(request);
                return new HttpResponseModel(lines[0], ReadHeaders(lines), ReadBody(request)); ;
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
                Headers.Add(new HttpHeaderModel("Set-Cookie", cookie));
            }
            else
                Headers.Add(new HttpHeaderModel("Set-Cookie", cookie));

            return cookie;
        }


        public string GetSessionCookie()
        {
            string receivedCookie = "";

            if (HasHeader("Set-Cookie"))
                receivedCookie = GetHeader("Set-Cookie").Value;
            return receivedCookie == "" ? "NO_SESSION" : receivedCookie;
        }

        public static HttpResponseModel Get500Error()
        {
            string firstLine = "HTTP/1.1 500 Internal Server Error";
            byte[] body = File.ReadAllBytes("../../../Assets/HTML/500.html");
            List<HttpHeaderModel> headers = new List<HttpHeaderModel>();

            headers.Add(new HttpHeaderModel("Content-Type", "text/html" + "\r\n"));

            HttpResponseModel httpMessage = new HttpResponseModel(firstLine, headers, body);

            return httpMessage;
        }
        public static HttpResponseModel Get503Error()
        {
            string firstLine = "HTTP/1.1 503 Service Unavailable";
            byte[] body = File.ReadAllBytes("../../../Assets/HTML/503.html");
            List<HttpHeaderModel> headers = new List<HttpHeaderModel>();

            headers.Add(new HttpHeaderModel("Content-Type", "text/html" + "\r\n"));

            HttpResponseModel httpMessage = new HttpResponseModel(firstLine, headers, body);

            return httpMessage;
        }
    }

}
