using System;
using System.Collections.Generic;
using System.Text;
using Domain.Interfaces;

namespace Domain.Models
{
    public class MeterData : IMeterData
    {
        public decimal Minimum { get; set; }
        public decimal Maximum { get; set; }
        public decimal Median { get; set; }
    }
}
