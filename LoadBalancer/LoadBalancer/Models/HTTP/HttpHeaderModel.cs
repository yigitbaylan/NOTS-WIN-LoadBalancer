using System;
using System.Collections.Generic;
using System.Text;

namespace LoadBalancer.Models.HTTP
{
    class HttpHeaderModel
    {
        public string Name { get; }
        public string Value { get; set; }

        public HttpHeaderModel(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Name}: {Value}";
        }
    }
}
