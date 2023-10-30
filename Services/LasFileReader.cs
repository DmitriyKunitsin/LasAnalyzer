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
            var graphDataForGamma = dataGenerator.GenerateGraphData(100);
            var graphDataForNeutronic = dataGenerator.GenerateGraphData(100);
            return (graphDataForGamma, graphDataForNeutronic);
        }
    }
}
