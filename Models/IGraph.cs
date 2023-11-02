using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LasAnalyzer.Models
{
    public interface IGraph
    {
        public ISeries[] ProbeSeries { get; set; }
        public LineSeries<double> LineSeries { get; set; }
        public List<double> Data { get; set; }
        public string Title { get; set; }

    }
}
