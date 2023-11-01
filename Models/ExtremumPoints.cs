using LiveChartsCore.Defaults;
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
    }
}
