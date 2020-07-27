using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Interfaces;

namespace AWSLambdaWebAPI.DTOs
{
    public class Response
    {
        public IMeterData LP { get; set; }

        public IMeterData TOU { get; set; }
    }
}
