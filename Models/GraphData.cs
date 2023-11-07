using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LasAnalyzer.Models
{
    public class GraphData
    {
        public List<double?> NearProbe { get; set; }
        public List<double?> FarProbe { get; set; }
        public List<double?> FarToNearProbeRatio { get; set; }
        public List<double?> Temperature { get; set; }
        public List<double?> Time { get; set; }
    }
}
