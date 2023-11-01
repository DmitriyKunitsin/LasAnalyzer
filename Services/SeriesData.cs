using LasAnalyzer.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Geo;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LasAnalyzer.Services
{
    public class SeriesData
    {
        public ISeries[] NearProbeSeries { get; set; }
        public ISeries[] FarProbeSeries { get; set; }
        public ISeries[] FarToNearProbeRatioSeries { get; set; }
        public ISeries[] TemperatureSeries { get; set; }

        public SeriesData()
        {
            NearProbeSeries = new ISeries[]
            {
                new LineSeries<double>()
            };

            FarProbeSeries = new ISeries[]
            {
                new LineSeries<double>()
            };

            FarToNearProbeRatioSeries = new ISeries[]
            {
                new LineSeries<double>()
            };

            TemperatureSeries = new ISeries[]
            {
                new LineSeries<double>()
            };
        }

        public SeriesData(GraphData graphData, int windowSize)
        {
            Calculator calculator = new Calculator();
            var baseHeatIndex = Utils.FindIndexForBaseValue(graphData.Temperature, TempType.Heating, windowSize);
            //var baseCoolIndex = Utils.FindIndexForBaseValue(graphData.Temperature, TempType.Cooling, windowSize);
            var baseHeatValues = calculator.InitializeBaseValues(graphData, TempType.Heating, baseHeatIndex.Value);
            var baseCoolValues = calculator.InitializeBaseValues(graphData, TempType.Cooling, baseHeatIndex.Value);
            var maxHeatPoint = calculator.FindExtremum(graphData.NearProbe, baseHeatIndex.Value, baseHeatValues.NearProbe, isMax: true);
            var minHeatPoint = calculator.FindExtremum(graphData.NearProbe, baseHeatIndex.Value, baseHeatValues.NearProbe, isMax: false);
            //var maxCoolPoint = calculator.FindExtremum(graphData.NearProbe, baseCoolIndex.Value, baseCoolValues.NearProbe, isMax: true);
            //var minCoolPoint = calculator.FindExtremum(graphData.NearProbe, baseCoolIndex.Value, baseCoolValues.NearProbe, isMax: false);
            NearProbeSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = graphData.NearProbe,
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
                },
                new ScatterSeries<ObservablePoint>
                {
                    Values = new ObservableCollection<ObservablePoint>
                    {
                        new ObservablePoint(baseHeatIndex, baseHeatValues.NearProbe),
                        new ObservablePoint(maxHeatPoint.Item1, maxHeatPoint.Item2),
                        new ObservablePoint(minHeatPoint.Item1, minHeatPoint.Item2),
                    },
                    Stroke = new SolidColorPaint
                    {
                        Color = SKColors.Red,
                        StrokeThickness = 3,
                        ZIndex = 1
                    },
                },
                //new ScatterSeries<ObservablePoint>
                //{
                //    Values = new ObservableCollection<ObservablePoint>
                //    {
                //        new ObservablePoint(baseCoolIndex, baseCoolValues.NearProbe),
                //        new ObservablePoint(maxCoolPoint.Item1, maxCoolPoint.Item2),
                //        new ObservablePoint(minCoolPoint.Item1, minCoolPoint.Item2),
                //    },
                //    Stroke = new SolidColorPaint
                //    {
                //        Color = SKColors.Blue,
                //        StrokeThickness = 3,
                //        ZIndex = 1
                //    },
                //},
            };
            FarProbeSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = graphData.FarProbe,
                    GeometryStroke = null,
                    GeometryFill = null,
                    Fill = null,
                    Stroke = new SolidColorPaint
                    {
                        Color = SKColors.BlueViolet,
                        StrokeThickness = 3,
                    },
                    LineSmoothness = 0
                }
            };

            FarToNearProbeRatioSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = graphData.FarToNearProbeRatio,
                    GeometryStroke = null,
                    GeometryFill = null,
                    Fill = null,
                    Stroke = new SolidColorPaint
                    {
                        Color = SKColors.BlueViolet,
                        StrokeThickness = 3,
                        ZIndex = 1
                    }
                }
            };
            TemperatureSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = graphData.Temperature,
                    GeometryStroke = null,
                    GeometryFill = null,
                    Fill = null,
                    Stroke = new SolidColorPaint
                    {
                        Color = SKColors.BlueViolet,
                        StrokeThickness = 3,
                    }
                }
            };
        }
    }
}
