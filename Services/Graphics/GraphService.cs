using LasAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LasAnalyzer.Services.Graphics
{
    public class GraphService
    {
        // при инициализации будут создаваться 4 объекта Graph
        // которые и будут представлять графики
        // в это классе будет определятья точка когда начинается охлаждение

        public ProbeGraph GraphNearProbe { get; set; }
        public ProbeGraph GraphFarProbe { get; set; }
        public ProbeGraph GraphFarToNearProbeRatio { get; set; }
        public TemperatureGraph GraphTemperature { get; set; }
        public TempType TemperatureType { get; set; }
        public int CoolingStartIndex { get; set; }

        public GraphService(GraphData graphData, (string, string) titles, int windowSize)
        {
            GraphTemperature = new TemperatureGraph(graphData.Temperature, "TEMPER", windowSize);

            CoolingStartIndex = GraphTemperature.CoolingStartIndex;
            TemperatureType = GraphTemperature.TemperatureType;

            var baseHeatIndex = GraphTemperature.BaseHeatIndex;
            var baseCoolIndex = GraphTemperature.BaseCoolIndex;

            GraphNearProbe = new ProbeGraph(graphData.NearProbe, titles.Item1, baseHeatIndex, baseCoolIndex);
            GraphFarProbe = new ProbeGraph(graphData.FarProbe, titles.Item2, baseHeatIndex, baseCoolIndex);
            GraphFarToNearProbeRatio = new ProbeGraph(graphData.FarToNearProbeRatio, $"{titles.Item2}/{titles.Item1}", baseHeatIndex, baseCoolIndex);

        }
    }
}
