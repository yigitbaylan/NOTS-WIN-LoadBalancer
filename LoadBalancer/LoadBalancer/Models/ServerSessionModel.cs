using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LoadBalancer.Models
{
    class ServerSessionModel
    {
        public string SessionID { get; set; }
        public string Server { get; set; }
        public DateTime Expires { get; set; }

        public ServerSessionModel(string id, string server, string HttpDate)
        {
            SessionID = id;
            Server = server;
            Expires = DateTime.ParseExact(HttpDate,
                    "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                    CultureInfo.InvariantCulture.DateTimeFormat,
                    DateTimeStyles.AssumeUniversal);
        }

        public bool IsExpired()
        {
            DateTime currentTime = DateTime.Now;
            return currentTime > Expires;
        }
    }
}
