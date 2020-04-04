using System;
using System.Collections.Generic;
using System.Text;

namespace HTTP
{
    public class HttpHeader
    {
        public string Name { get; }
        public string Value { get; set; }

        public HttpHeader(string name, string value)
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
