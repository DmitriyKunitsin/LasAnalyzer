using LasAnalyzer.Models;
using System;
using System.Collections.Generic;

namespace LasAnalyzer.Services
{
    public class DataGenerator
    {
        public GraphData GenerateGraphData(int dataPoints)
        {
            var graphData = new GraphData
            {
                NearProbe = GenerateRandomData(dataPoints, minValue: 80, maxValue: 100),
                FarProbe = GenerateRandomData(dataPoints, minValue: 80, maxValue: 100),
                FarToNearProbeRatio = GenerateRandomData(dataPoints, minValue: 1, maxValue: 2),
                Temperature = GenerateRandomData(dataPoints, minValue: 20, maxValue: 120),
                Time = GenerateTimeData(dataPoints, intervalSeconds: 60)
            };

            return graphData;
        }

        private List<double> GenerateRandomData(int count, double minValue, double maxValue)
        {
            var random = new Random();
            var data = new List<double>();
            for (int i = 0; i < count; i++)
            {
                data.Add(random.NextDouble() * (maxValue - minValue) + minValue);
            }
            return data;
        }

        private List<double> GenerateTimeData(int count, int intervalSeconds)
        {
            var timeData = new List<double>();
            for (int i = 0; i < count; i++)
            {
                timeData.Add(i * intervalSeconds);
            }
            return timeData;
        }
    }
}
