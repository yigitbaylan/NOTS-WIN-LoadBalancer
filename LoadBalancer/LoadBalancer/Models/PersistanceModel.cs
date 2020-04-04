using System;
using System.Collections.Generic;
using System.Text;

namespace LoadBalancer.Models
{
    class PersistanceModel
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public PersistanceModel(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}
