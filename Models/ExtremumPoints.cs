using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LasAnalyzer.Models
{
    public class ExtremumPoints
    {
        public ObservablePoint BasePoint { get; set; }
        public ObservablePoint MaxPoint { get; set; }
        public ObservablePoint MinPoint { get; set; }

        public ScatterSeries<ObservablePoint> ScatterBasePoint { get; set; }
        public ScatterSeries<ObservablePoint> ScatterMaxPoint { get; set; }
        public ScatterSeries<ObservablePoint> ScatterMinPoint { get; set; }

        public ExtremumPoints()
        {
            ScatterBasePoint = new ScatterSeries<ObservablePoint>();
            ScatterMaxPoint = new ScatterSeries<ObservablePoint>();
            ScatterMinPoint = new ScatterSeries<ObservablePoint>();
        }
    }
}
