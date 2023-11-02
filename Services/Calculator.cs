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

            var maxExtrema = GetMaximums(nearProbeExtrema, farProbeExtrema, farToNearProbeExtrema);
            var minExtrema = GetMinimums(nearProbeExtrema, farProbeExtrema, farToNearProbeExtrema);

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
                TemperBase = graphService.GraphTemperature.Data[baseIndex]
            };

            return resultTable;
        }

        private Result GetBaseValues(ExtremumPoints nearProbeExtrema, ExtremumPoints farProbeExtrema, ExtremumPoints farToNearProbeExtrema)
        {
            var result = new Result();

            result.NearProbe = nearProbeExtrema.BasePoint.Y.Value;
            result.FarProbe = farProbeExtrema.BasePoint.Y.Value;
            result.FarToNearProbeRatio = farToNearProbeExtrema.BasePoint.Y.Value;

            return result;
        }

        private Result GetMaximums(ExtremumPoints nearProbeExtrema, ExtremumPoints farProbeExtrema, ExtremumPoints farToNearProbeExtrema)
        {
            var result = new Result();

            result.NearProbe = nearProbeExtrema.MaxPoint.Y.Value;
            result.FarProbe = farProbeExtrema.MaxPoint.Y.Value;
            result.FarToNearProbeRatio = farToNearProbeExtrema.MaxPoint.Y.Value;

            return result;
        }
        private Result GetMinimums(ExtremumPoints nearProbeExtrema, ExtremumPoints farProbeExtrema, ExtremumPoints farToNearProbeExtrema)
        {
            var result = new Result();

            result.NearProbe = nearProbeExtrema.MinPoint.Y.Value;
            result.FarProbe = farProbeExtrema.MinPoint.Y.Value;
            result.FarToNearProbeRatio = farToNearProbeExtrema.MinPoint.Y.Value;

            return result;
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
