using LasAnalyzer.Services.Graphics;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LasAnalyzer.Services;

namespace LasAnalyzer.Models
{
    public class ReportModel
    {
        public string SerialNumber { get; set; }
        public string DeviceType { get; set; }
        public string TestDate { get; set; }
        public string NearProbeThreshold { get; set; }
        public string FarProbeThreshold { get; set; }
        public string NearProbeTitle { get; set; }
        public string FarProbeTitle { get; set; }
        public List<byte[]> Graphs { get; set; }
        public List<ResultTable> Results { get; set; }
        public string Conclusion { get; set; }
    }
}
