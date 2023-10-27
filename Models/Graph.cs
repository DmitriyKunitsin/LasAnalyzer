using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xceed.Document.NET;

namespace LasAnalyzer.Models
{
    public class Graph : Chart
    {
        public string Title { get; set; }
        public Chart Data { get; set; }

        protected override XElement CreateExternalChartXml()
        {
            throw new NotImplementedException();
        }

        protected override XElement GetChartTypeXElement()
        {
            throw new NotImplementedException();
        }
    }
}
