using LasAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LasAnalyzer.Services
{
    public class LasFileReader
    {
        public (GraphData, GraphData) OpenLasFile(string filePath)
        {
            DataGenerator dataGenerator = new DataGenerator();
            var graphDataForGamma = dataGenerator.GenerateGraphData(1000);
            var graphDataForNeutronic = dataGenerator.GenerateGraphData(1000);
            return (graphDataForGamma, graphDataForNeutronic);
        }
    }
}
