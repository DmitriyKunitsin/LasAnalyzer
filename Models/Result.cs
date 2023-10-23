using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LasAnalyzer.Models
{
    public class Result
    {
        public int Num { get; set; }
        public string Formula { get; set; }
        public double NearProbe { get; set; }
        public double FarProbe { get; set; }
        public double FarToNearProbeRatio { get; set; }
    }
}
