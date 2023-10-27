using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LasAnalyzer.Models
{
    public class ReportModel
    {
        public string SerialNumber { get; set; }
        public string DeviceType { get; set; }
        public string TestDate { get; set; }
        public double NearProbeThreshold { get; set; }
        public double FarProbeThreshold { get; set; }
        public GraphData Graphs { get; set; }
        public List<Result> Results { get; set; }
        public string Conclusion { get; set; }
    }
}
