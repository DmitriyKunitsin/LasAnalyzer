using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LasAnalyzer.Models
{
    public class ResultTable
    {
        public List<Result> Results { get; set; }
        public TempType TempType { get; set; }
        public double TemperBase { get; set; }
    }
}
