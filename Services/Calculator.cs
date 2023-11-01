using LasAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LasAnalyzer.Services
{
    public class Calculator
    {
        public ResultTable CalculateMetrics(GraphData graphData, TempType tempType, int windowSize)
        {
            var baseIndex = Utils.FindIndexForBaseValue(graphData.Temperature, tempType, windowSize);

            if (baseIndex is null)
                return null;

            var baseValues = InitializeBaseValues(graphData, tempType, baseIndex.Value);

            var maxExtrema = GetExtremums(graphData, baseValues, baseIndex.Value);
            var minExtrema = GetExtremums(graphData, baseValues, baseIndex.Value, isMax: false);

            var maxDif = GetDifferences(baseValues, maxExtrema);
            var minDif = GetDifferences(baseValues, minExtrema);

            var maxPercent = GetPercentage(baseValues, maxDif);
            var minPercent = GetPercentage(baseValues, minDif);

            var results = new List<Result>()
            {
                baseValues,
                maxExtrema,
                minExtrema,
                maxDif,
                minDif,
                maxPercent,
                minPercent
            };

            var resultTable = new ResultTable()
            {
                Results = results,
                TempType = tempType,
                TemperBase = graphData.Temperature[baseIndex.Value]
            };

            return resultTable;
        }

        public Result InitializeBaseValues(GraphData graphData, TempType tempType, int indexForBaseValue)
        {
            var result = new Result();

            result.Num = 1;
            result.Formula = $"N(T={graphData.Temperature[indexForBaseValue]})";

            if (tempType == TempType.Heating)
            {
                result.NearProbe = graphData.NearProbe.Take(indexForBaseValue).Average();
                result.FarProbe = graphData.FarProbe.Take(indexForBaseValue).Average();
                result.FarToNearProbeRatio = graphData.FarToNearProbeRatio.Take(indexForBaseValue).Average();
            }
            else if (tempType == TempType.Cooling)
            {
                result.NearProbe = graphData.NearProbe.Skip(indexForBaseValue).Average();
                result.FarProbe = graphData.FarProbe.Skip(indexForBaseValue).Average();
                result.FarToNearProbeRatio = graphData.FarToNearProbeRatio.Skip(indexForBaseValue).Average();
            }

            return result;
        }

        private Result GetExtremums(GraphData graphData, Result baseValues, int baseIndex, bool isMax = true)
        {
            var result = new Result();

            var temperatures = new Temperatures();

            result.Num = isMax ? 2 : 3;
            result.Formula = isMax ? "MAX/T" : "MIN/T";

            var extremPoint = FindExtremum(graphData.NearProbe, baseIndex, baseValues.NearProbe, isMax);
            result.NearProbe = extremPoint.Item2;
            temperatures.NearProbe = graphData.Temperature[extremPoint.Item1];

            extremPoint = FindExtremum(graphData.FarProbe, baseIndex, baseValues.FarProbe, isMax);
            result.FarProbe = extremPoint.Item2;
            temperatures.FarProbe = graphData.Temperature[extremPoint.Item1];

            extremPoint = FindExtremum(graphData.FarToNearProbeRatio, baseIndex, baseValues.FarToNearProbeRatio, isMax);
            result.FarToNearProbeRatio = extremPoint.Item2;
            temperatures.FarToNearProbeRatio = graphData.Temperature[extremPoint.Item1];

            result.Temperatures = temperatures;

            return result;
        }

        public (int, double) FindExtremum(List<double> probePoints, int baseIndex, double baseValue, bool isMax = true)
        {
            var extremumIndex = isMax ? probePoints.IndexOf(probePoints.Max()) : probePoints.IndexOf(probePoints.Min());
            var extremum = probePoints[extremumIndex];

            if (CalculateDeviation(extremum, baseValue))
            {
                extremumIndex = baseIndex;
                extremum = baseValue;
            }

            return (extremumIndex, extremum);
        }

        private bool CalculateDeviation(double value, double baseValue, double threshold = 0.005)
        {
            return (value - baseValue) / baseValue < threshold;
        }

        private Result GetDifferences(Result baseValues, Result Extrema)
        {
            var result = new Result();

            result.NearProbe = Extrema.NearProbe - baseValues.NearProbe;
            result.FarProbe = Extrema.FarProbe - baseValues.FarProbe;
            result.FarToNearProbeRatio = Extrema.FarToNearProbeRatio - baseValues.FarToNearProbeRatio;

            return result;
        }

        private Result GetPercentage(Result baseValues, Result differences)
        {
            var result = new Result();

            result.NearProbe = differences.NearProbe / baseValues.NearProbe;
            result.FarProbe = differences.FarProbe / baseValues.FarProbe;
            result.FarToNearProbeRatio = differences.FarToNearProbeRatio / baseValues.FarToNearProbeRatio;

            return result;
        }
    }
}
