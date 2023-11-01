using LasAnalyzer.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LasAnalyzer.Services
{
    public class Graph
    {
        public ISeries[] ProbeSeries { get; set; }
        public LineSeries<double> LineSeries { get; set; }
        public ScatterSeries<ObservablePoint> HeatScatterSeries { get; set; }
        public ScatterSeries<ObservablePoint> CoolScatterSeries { get; set; }
        public List<double> Data { get; set; }
        public string Title { get; set; }
        public int WindowSize { get; set; }

        // Points
        public ExtremumPoints HeatingExtremumPoints { get; set; }
        public ExtremumPoints CoolingExtremumPoints { get; set; }

        public Graph(List<double> data, string title, int windowSize)
        {
            Data = data;
            Title = title;
            WindowSize = windowSize;

            LineSeries = new LineSeries<double>
            {
                Values = data,
                GeometryStroke = null,
                GeometryFill = null,
                Fill = null,
                Stroke = new SolidColorPaint
                {
                    Color = SKColors.BlueViolet,
                    StrokeThickness = 3,
                    ZIndex = 1
                },
                ZIndex = 1,
            };

            HeatingExtremumPoints = FindExtremum(TempType.Heating);
            CoolingExtremumPoints = FindExtremum(TempType.Cooling);

            HeatScatterSeries = new ScatterSeries<ObservablePoint>
            {
                Values = new ObservableCollection<ObservablePoint>
                    {
                        HeatingExtremumPoints.BasePoint,
                        HeatingExtremumPoints.MaxPoint,
                        HeatingExtremumPoints.MinPoint,
                    },
                Stroke = new SolidColorPaint
                {
                    Color = SKColors.Red,
                    StrokeThickness = 3,
                    ZIndex = 1
                },
            };

            CoolScatterSeries = new ScatterSeries<ObservablePoint>
            {
                Values = new ObservableCollection<ObservablePoint>
                    {
                        CoolingExtremumPoints.BasePoint,
                        CoolingExtremumPoints.MaxPoint,
                        CoolingExtremumPoints.MinPoint,
                    },
                Stroke = new SolidColorPaint
                {
                    Color = SKColors.Blue,
                    StrokeThickness = 3,
                    ZIndex = 1
                },
            };

            ProbeSeries = new ISeries[]
            {
                LineSeries,
                HeatScatterSeries,
                CoolScatterSeries,
            };
        }

        private ExtremumPoints FindExtremum(TempType tempType)
        {
            Calculator calculator = new Calculator();
            var baseHeatIndex = Utils.FindIndexForBaseValue(data, TempType.Heating, windowSize);
            //var baseCoolIndex = Utils.FindIndexForBaseValue(graphData.Temperature, TempType.Cooling, windowSize);
            var baseHeatValues = calculator.InitializeBaseValues(graphData, TempType.Heating, baseHeatIndex.Value);
            var baseCoolValues = calculator.InitializeBaseValues(graphData, TempType.Cooling, baseHeatIndex.Value);
            var maxHeatPoint = calculator.FindExtremum(graphData.NearProbe, baseHeatIndex.Value, baseHeatValues.NearProbe, isMax: true);
            var minHeatPoint = calculator.FindExtremum(graphData.NearProbe, baseHeatIndex.Value, baseHeatValues.NearProbe, isMax: false);

            FindBaseValues();
            return new ExtremumPoints();
        }

        public Result FindBaseValues(GraphData graphData, TempType tempType, int indexForBaseValue)
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

        private (int, double) FindMaxExtremum()
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

        private (int, double) FindMinExtremum()
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
    }
}
