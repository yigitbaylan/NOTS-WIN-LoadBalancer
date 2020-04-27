using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LoadBalancer.Models
{
    class SessionModel
    {
        public string SessionID { get; set; }
        public string Server { get; set; }
        public DateTime Expires { get; set; }

        public SessionModel(string id, string server, string HttpDate)
        {
            SessionID = id;
            Server = server;
            Expires = DateTime.ParseExact(HttpDate,
                    "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                    CultureInfo.InvariantCulture.DateTimeFormat,
                    DateTimeStyles.AssumeUniversal);
        }
        /// <summary>
        /// Check if the session is expired
        /// </summary>
        /// <returns></returns>
        public bool IsExpired()
        {
            DateTime currentTime = DateTime.UtcNow;
            return currentTime > Expires;
        }
    }
}
