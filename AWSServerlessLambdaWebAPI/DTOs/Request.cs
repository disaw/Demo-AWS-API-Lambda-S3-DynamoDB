using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSLambdaWebAPI.DTOs
{
    public class Request
    {
        public string Date { get; set; }
        public int Meter { get; set; }
        public int DataType { get; set; }
    }
}
