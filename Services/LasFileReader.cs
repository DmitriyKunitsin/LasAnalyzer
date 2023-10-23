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
        public GraphData OpenLasFile(string filePath)
        {
            DataGenerator dataGenerator = new DataGenerator();
            var graphData = dataGenerator.GenerateGraphData(100);
            return graphData;
        }
    }
}
