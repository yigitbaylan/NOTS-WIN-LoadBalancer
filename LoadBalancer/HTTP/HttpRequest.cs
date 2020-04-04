using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTTP
{
    public class HttpRequest : HttpMessage
    {
        public HttpRequest(string firstline, List<HttpHeader> headers, byte[] body) : base(firstline, headers, body)
        {
        }

        public static HttpRequest Parse(byte[] request)
        {
            try
            {
                if (request.Count() == 0)
                    throw new Exception();
                var lines = ReadLines(request);
                return new HttpRequest(lines[0], ReadHeaders(lines), ReadBody(request));
            }
            catch
            {
                throw new FormatException();
            }
        }
        public string GetRequestedHost()
        {
            if (HasHeader("Host"))
            {
                return GetHeader("Host").Value;
            }
            return FirstLine.Split(' ')[1];
        }

        public string GetCookie()
        {
            if (HasHeader("Cookie"))
            {
                List<string> connectedServerList = GetHeader("Cookie").Value.Split(";").Where(cookie => cookie.Contains("ConnectedServer")).ToList();
                return connectedServerList.Count == 0 ? "NO_COOKIE" : connectedServerList[0].Split("=")[1];
            }
            return "NO_COOKIE";
        }

        public string GetServerSessionID()
        {
            if (HasHeader("Cookie"))
            {
                List<string> connectedServerList = GetHeader("Cookie").Value.Split(";").Where(cookie => cookie.Contains("connect.sid")).ToList();
                return connectedServerList.Count == 0 ? "NO_SESSION" : connectedServerList[0].Split("=")[1];

            }
            return "NO_SESSION";
        }

        public string OnlyAskForHeaders()
        {
            int i = FirstLine.IndexOf(" ") + 1;
            string str = FirstLine.Substring(i);
            return "HEAD " + str;
        }

        public bool CheckIfHTTPRquestIsImage()
        {
            string[] FileTypes = new string[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".tiff", ".bmp", ".eps", ".raw", ".ai", ".pdf", ".psd" };
            return FileTypes.Any(type => FirstLine.Split(' ')[1].ToLower().Contains(type));
        }
    }
}
