using LasAnalyzer.Models;
using LasAnalyzer.Services.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LasAnalyzer.Services
{
    public class Calculator
    {
        public ResultTable CalculateMetrics(GraphService graphService, TempType tempType)
        {
            if (graphService is null) return null;

            ExtremumPoints nearProbeExtrema = new ExtremumPoints();
            ExtremumPoints farProbeExtrema = new ExtremumPoints();
            ExtremumPoints farToNearProbeExtrema = new ExtremumPoints();

            if (tempType == TempType.Heating)
            {
                nearProbeExtrema = graphService.GraphNearProbe.HeatingExtremumPoints;
                farProbeExtrema = graphService.GraphFarProbe.HeatingExtremumPoints;
                farToNearProbeExtrema = graphService.GraphFarToNearProbeRatio.HeatingExtremumPoints;
            }
            else if (tempType == TempType.Cooling)
            {
                nearProbeExtrema = graphService.GraphNearProbe.CoolingExtremumPoints;
                farProbeExtrema = graphService.GraphFarProbe.CoolingExtremumPoints;
                farToNearProbeExtrema = graphService.GraphFarToNearProbeRatio.CoolingExtremumPoints;
            }

            var baseValues = GetBaseValues(nearProbeExtrema, farProbeExtrema, farToNearProbeExtrema);

            var maxExtrema = GetMaximums(nearProbeExtrema, farProbeExtrema, farToNearProbeExtrema, graphService.GraphTemperature.Data);
            var minExtrema = GetMinimums(nearProbeExtrema, farProbeExtrema, farToNearProbeExtrema, graphService.GraphTemperature.Data);

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

            var baseIndex = 0;

            if (tempType == TempType.Heating)
            {
                baseIndex = graphService.GraphTemperature.BaseHeatIndex;
            }
            else if (tempType == TempType.Cooling)
            {
                baseIndex = graphService.GraphTemperature.BaseCoolIndex;
            }

            var resultTable = new ResultTable()
            {
                Results = results,
                TempType = tempType,
                TemperBase = graphService.GraphTemperature.Data[baseIndex],
                ThresholdExceeded = IsThresholdExceeded(results)
            };

            return resultTable;
        }

        private bool IsThresholdExceeded(List<Result> results)
        {
            var threshold = 5;
            if (
                Math.Abs(results[5].NearProbe) > threshold || Math.Abs(results[6].NearProbe) > threshold ||
                Math.Abs(results[5].FarProbe) > threshold || Math.Abs(results[6].FarProbe) > threshold ||
                Math.Abs(results[5].FarToNearProbeRatio) > threshold || Math.Abs(results[6].FarToNearProbeRatio) > threshold
            )
            {
                return true;
            }
            return false;
        }

        private Result GetBaseValues(ExtremumPoints nearProbeExtrema, ExtremumPoints farProbeExtrema, ExtremumPoints farToNearProbeExtrema)
        {
            var result = new Result();

            result.NearProbe = Math.Round(nearProbeExtrema.BasePoint.Y.Value, 3);
            result.FarProbe = Math.Round(farProbeExtrema.BasePoint.Y.Value, 3);
            result.FarToNearProbeRatio = Math.Round(farToNearProbeExtrema.BasePoint.Y.Value, 3);

            return result;
        }

        private Result GetMaximums(ExtremumPoints nearProbeExtrema, ExtremumPoints farProbeExtrema, ExtremumPoints farToNearProbeExtrema, List<double?> tempData)
        {
            var result = new Result();
            result.Temperatures = new Temperatures();

            result.NearProbe = Math.Round(nearProbeExtrema.MaxPoint.Y.Value, 3);
            result.FarProbe = Math.Round(farProbeExtrema.MaxPoint.Y.Value, 3);
            result.FarToNearProbeRatio = Math.Round(farToNearProbeExtrema.MaxPoint.Y.Value, 3);

            result.Temperatures.NearProbe = tempData[Convert.ToInt32(nearProbeExtrema.MaxPoint.X)].Value;
            result.Temperatures.FarProbe = tempData[Convert.ToInt32(farProbeExtrema.MaxPoint.X)].Value;
            result.Temperatures.FarToNearProbeRatio = tempData[Convert.ToInt32(farToNearProbeExtrema.MaxPoint.X)].Value;

            return result;
        }
        private Result GetMinimums(ExtremumPoints nearProbeExtrema, ExtremumPoints farProbeExtrema, ExtremumPoints farToNearProbeExtrema, List<double?> tempData)
        {
            var result = new Result();
            result.Temperatures = new Temperatures();

            result.NearProbe = Math.Round(nearProbeExtrema.MinPoint.Y.Value, 3);
            result.FarProbe = Math.Round(farProbeExtrema.MinPoint.Y.Value, 3);
            result.FarToNearProbeRatio = Math.Round(farToNearProbeExtrema.MinPoint.Y.Value, 3);

            result.Temperatures.NearProbe = tempData[Convert.ToInt32(nearProbeExtrema.MinPoint.X)].Value;
            result.Temperatures.FarProbe = tempData[Convert.ToInt32(farProbeExtrema.MinPoint.X)].Value;
            result.Temperatures.FarToNearProbeRatio = tempData[Convert.ToInt32(farToNearProbeExtrema.MinPoint.X)].Value;

            return result;
        }


        private Result GetDifferences(Result baseValues, Result Extrema)
        {
            var result = new Result();

            result.NearProbe = Math.Round(Extrema.NearProbe - baseValues.NearProbe, 3);
            result.FarProbe = Math.Round(Extrema.FarProbe - baseValues.FarProbe, 3);
            result.FarToNearProbeRatio = Math.Round(Extrema.FarToNearProbeRatio - baseValues.FarToNearProbeRatio, 3);

            return result;
        }

        private Result GetPercentage(Result baseValues, Result differences)
        {
            var result = new Result();

            result.NearProbe = Math.Round((differences.NearProbe / baseValues.NearProbe) * 100, 2);
            result.FarProbe = Math.Round((differences.FarProbe / baseValues.FarProbe) * 100, 2);
            result.FarToNearProbeRatio = Math.Round((differences.FarToNearProbeRatio / baseValues.FarToNearProbeRatio) * 100, 2);

            return result;
        }
    }
}
