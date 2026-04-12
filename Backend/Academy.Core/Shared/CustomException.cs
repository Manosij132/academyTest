using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academy.Core.Shared
{
    public class CustomException : Exception
    {
        public string ResponseData { get; set; }
        public CustomException(string message) : base(message) { }
        public CustomException(string message, string data) : base(message)
        {
            ResponseData = data;
        }

    }
}
