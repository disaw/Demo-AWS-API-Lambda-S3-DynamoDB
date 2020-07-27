using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Configuration
    {
        public string CSVBucket { get; set; }
        public string LPFilePrefix { get; set; }
        public string TOUFilePrefix { get; set; }
        public char NewLine { get; set; }
        public char Separator { get; set; }

        public string LPTableName { get; set; }
        public string TOUTableName { get; set; }
    }
}
