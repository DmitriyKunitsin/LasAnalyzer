using LasAnalyzer.Models;
using Looch.LasParser;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LasAnalyzer.Services
{
    public class LasFileReader
    {
        public (GraphData, GraphData) OpenLasFile(string filePath)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            LasParserVlasov lasParserVlasov = new LasParserVlasov();
            lasParserVlasov.ReadFile(filePath, "windows-1251");
            var rsd = lasParserVlasov.Data["RSD"].ToList();
            var rld = lasParserVlasov.Data["RLD"];
            var temper = lasParserVlasov.Data["MT"];

            var graphData = new GraphData
            {
                NearProbe = rsd,
                FarProbe = rld,
                FarToNearProbeRatio = rsd,
                Temperature = temper,
                Time = temper
            };

            DataGenerator dataGenerator = new DataGenerator();
            var graphDataForGamma = dataGenerator.GenerateGraphData(1000);
            var graphDataForNeutronic = dataGenerator.GenerateGraphData(1000);
            return (graphDataForGamma, graphDataForNeutronic);
        }
    }
}
