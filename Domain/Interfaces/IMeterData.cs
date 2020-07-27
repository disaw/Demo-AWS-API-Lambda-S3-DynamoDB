using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface IMeterData
    {
        public decimal Minimum { get; set; }
        public decimal Maximum { get; set; }
        public decimal Median { get; set; }
    }
}
