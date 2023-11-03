﻿using LasAnalyzer.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ReactiveUI;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace LasAnalyzer.Services.Graphics
{
    public class ProbeGraph : IGraph
    {
        public ISeries[] ProbeSeries { get; set; }
        public LineSeries<double> LineSeries { get; set; }
        public ScatterSeries<ObservablePoint> HeatScatterSeries { get; set; }
        public ScatterSeries<ObservablePoint> CoolScatterSeries { get; set; }
        public List<double> Data { get; set; }
        public string Title { get; set; }

        // Points
        public ExtremumPoints HeatingExtremumPoints { get; set; }
        public ExtremumPoints CoolingExtremumPoints { get; set; }

        public RectangularSection[] Thumbs { get; set; }

        public ProbeGraph(string title)
        {
            Title = title;

            LineSeries = new LineSeries<double>();

            ProbeSeries = new ISeries[]
            {
                LineSeries,
            };

            Thumbs = new[]
            {
                new RectangularSection
                {
                    Fill = new SolidColorPaint(new SKColor(255, 205, 210, 100)),
                    Xi = 0,
                    Xj = 0,
                    Stroke = new SolidColorPaint
                    {
                        Color = SKColors.Red,
                        StrokeThickness = 3,
                        ZIndex = 2
                    }
                }
            };
        }

        public ProbeGraph(List<double> data, string title, int coolingStartIndex, int baseHeatIndex, int baseCoolIndex)
        {
            Data = data;
            Title = title;

            Thumbs = new[]
            {
                new RectangularSection
                {
                    Fill = new SolidColorPaint(new SKColor(255, 205, 210, 100)),
                    Xi = 0,
                    Xj = 0,
                    Stroke = new SolidColorPaint
                    {
                        Color = SKColors.Red,
                        StrokeThickness = 3,
                        ZIndex = 2
                    }
                }
            };

            LineSeries = new LineSeries<double>
            {
                Values = data,
                GeometryStroke = null,
                GeometryFill = null,
                Fill = null,
                Stroke = new SolidColorPaint
                {
                    Color = SKColors.RoyalBlue,
                    StrokeThickness = 3,
                    ZIndex = 1
                },
                ZIndex = 1,
            };


            if (baseHeatIndex != -1)
            {
                HeatingExtremumPoints = FindExtremum(
                    data.Take(coolingStartIndex).ToList(),
                    TempType.Heating,
                    baseHeatIndex
                );
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
                    GeometrySize = 10
                };
            }

            if (baseCoolIndex != -1)
            {
                CoolingExtremumPoints = FindExtremum(
                    data.Skip(coolingStartIndex).ToList(),
                    TempType.Cooling,
                    baseCoolIndex
                );
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
                    GeometrySize = 10
                };
            }

            ProbeSeries = new ISeries[]
            {
                LineSeries,
                HeatScatterSeries ?? new ScatterSeries<ObservablePoint>(),
                CoolScatterSeries ?? new ScatterSeries<ObservablePoint>(),
            };
        }

        public void PointerDown(PointerCommandArgs args)
        {
            // при наведении на точку она чуть увеличивается, за нее можно схватиться ЛКМ
            // мб флаг определяющий что конкретная точка схвачена

            //HeatingExtremumPoints;
            //CoolingExtremumPoints;
            //if (true)
            //{

            //}
            throw new NotImplementedException();
        }

        public void PointerMove(PointerCommandArgs args)
        {
            // после прожатия ЛКМ точку можно перемещать
            throw new NotImplementedException();
        }

        public void PointerUp(PointerCommandArgs args)
        {
            throw new NotImplementedException();
        }

        private ExtremumPoints FindExtremum(List<double> data, TempType tempType, int baseIndex)
        {
            ExtremumPoints ExtremumPoints = new ExtremumPoints();

            ExtremumPoints.BasePoint = FindBaseValues(data, tempType, baseIndex);
            ExtremumPoints.MaxPoint = FindMaxExtremum(data, ExtremumPoints.BasePoint);
            ExtremumPoints.MinPoint = FindMinExtremum(data, ExtremumPoints.BasePoint);

            return ExtremumPoints;
        }

        public ObservablePoint FindBaseValues(List<double> data, TempType tempType, int baseIndex)
        {
            var basePoint = new ObservablePoint();
            basePoint.X = baseIndex;

            if (tempType == TempType.Heating)
            {
                baseIndex = baseIndex == 0 ? 1 : baseIndex;
                basePoint.Y = data.Take(baseIndex).Average();
            }
            else if (tempType == TempType.Cooling)
            {
                baseIndex = baseIndex == data.Count - 1 ? data.Count - 2 : baseIndex;
                basePoint.Y = data.Skip(baseIndex).Average();
            }

            return basePoint;
        }

        private ObservablePoint FindMaxExtremum(List<double> data, ObservablePoint basePoint)
        {
            var maxPoint = new ObservablePoint();
            var maxIdx = data.IndexOf(data.Max());
            var max = data[maxIdx];

            if (IsExceedThreshold(max, basePoint.Y.Value))
            {
                maxPoint.X = maxIdx;
                maxPoint.Y = max;
            }
            else
            {
                maxPoint.X = basePoint.X.Value;
                maxPoint.Y = basePoint.Y.Value;
            }

            return maxPoint;
        }

        private ObservablePoint FindMinExtremum(List<double> data, ObservablePoint basePoint)
        {
            var minPoint = new ObservablePoint();
            var minIdx = data.IndexOf(data.Min());
            var min = data[minIdx];

            if (IsExceedThreshold(min, basePoint.Y.Value))
            {
                minPoint.X = minIdx;
                minPoint.Y = min;
            }
            else
            {
                minPoint.X = basePoint.X.Value;
                minPoint.Y = basePoint.Y.Value;
            }

            return minPoint;
        }

        private bool IsExceedThreshold(double value, double baseValue, double threshold = 0.005)
        {
            return Math.Abs((value - baseValue) / baseValue) > threshold;
        }
    }
}
