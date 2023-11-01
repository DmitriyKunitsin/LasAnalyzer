using LasAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LasAnalyzer.Services
{
    public class GraphService
    {
        // при инициализации будут создаваться 4 объекта Graph
        // которые и будут представлять графики
        // в это классе будет определятья точка когда начинается охлаждение
        // продумать момент когда только нагрев или только охлад
        
        public Graph NearProbe { get; set; }
        public Graph FarProbe { get; set; }
        public Graph FarToNearProbeRatio { get; set; }
        public Graph Temperature { get; set; }
        public TempType TemperatureType { get; set; }

        public GraphService(GraphData graphData, (string, string) titles, int windowSize)
        {
            var baseHeatIndex = Utils.FindIndexForBaseValue(graphData.Temperature, TempType.Heating, windowSize);

            NearProbe = new Graph(graphData.NearProbe, titles.Item1, windowSize);
            FarProbe = new Graph(graphData.NearProbe, titles.Item2, windowSize);
            FarToNearProbeRatio = new Graph(graphData.NearProbe, $"{titles.Item2}/{titles.Item1}", windowSize);
            Temperature = new Graph(graphData.NearProbe, "TEMPER", windowSize);
        }

    }
}
