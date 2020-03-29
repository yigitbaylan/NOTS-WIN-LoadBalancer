using System;
using System.Collections.Generic;
using System.Text;

namespace LoadBalancer.Models
{
    class LogModel
    {
        public string Type { get; set; }
        public string Time { get; set; }
        public string Content{ get; set; }

        public LogModel(string type, string content)
        {
            Type = type;
            Time = DateTime.Now.ToString();
            Content = content;
        }
    }
}
