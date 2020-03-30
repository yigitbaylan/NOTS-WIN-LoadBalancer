using System;
using System.Collections.Generic;
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

        public HttpResponseModel GetAuthenticationResponse()
        {
            string firstLine = "HTTP/1.1 407 Proxy Authentication Required";
            List<HttpHeaderModel> headers = new List<HttpHeaderModel>();
            byte[] body = new byte[0];
            headers.Add(new HttpHeaderModel("Proxy-Authenticate", "Basic realm=\"Proxy\""));
            headers.Add(new HttpHeaderModel("Content-Length", "0\r\n"));

            HttpResponseModel httpMessage = new HttpResponseModel(firstLine, headers, body);
            return httpMessage;
        }

        public HttpResponseModel Get500Error()
        {
            string firstLine = "HTTP/1.1 500 Proxy Error";
            byte[] body = File.ReadAllBytes("../../Assets/HTML/500.html");
            List<HttpHeaderModel> headers = new List<HttpHeaderModel>();

            headers.Add(new HttpHeaderModel("Content-Type", "text/html" + "\r\n"));

            HttpResponseModel httpMessage = new HttpResponseModel(firstLine, headers, body);

            return httpMessage;
        }
    }

}
