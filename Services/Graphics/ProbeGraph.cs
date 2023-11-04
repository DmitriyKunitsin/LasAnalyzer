using Avalonia.Controls.Primitives;
using LasAnalyzer.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
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

        private RectangularSection[] _thumbs;
        public RectangularSection[] Thumbs
        {
            get => _thumbs;
            set
            {
                _thumbs[0].Xi = value[0].Xi;
                _thumbs[0].Xj = value[0].Xj;
            }
        }

        public ReactiveCommand<PointerCommandArgs, Unit> PointerDownCommand { get; }
        public ReactiveCommand<PointerCommandArgs, Unit> PointerMoveCommand { get; }
        public ReactiveCommand<PointerCommandArgs, Unit> PointerUpCommand { get; }

        public ProbeGraph(RectangularSection[] thumbs, string title)
        {
            Title = title;

            LineSeries = new LineSeries<double>();

            ProbeSeries = new ISeries[]
            {
                LineSeries,
            };

            _thumbs = new[]
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

            Thumbs = thumbs;
        }

        public ProbeGraph(List<double> data, RectangularSection[] thumbs, string title, int coolingStartIndex, int baseHeatIndex, int baseCoolIndex)
        {
            Data = data;
            Title = title;

            _thumbs = new[]
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

            Thumbs = thumbs;

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
                    Fill = new SolidColorPaint(new SKColor(229, 57, 53, 100)),
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
                    Fill = new SolidColorPaint(new SKColor(25, 118, 210, 100)),
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

            PointerDownCommand = ReactiveCommand.Create<PointerCommandArgs>(PointerDown);
            PointerMoveCommand = ReactiveCommand.Create<PointerCommandArgs>(PointerMove);
            PointerUpCommand = ReactiveCommand.Create<PointerCommandArgs>(PointerUp);
        }

        public ObservablePoint GetNearlyExtrema(LvcPointD lastPointerPosition)
        {
            double dx = HeatingExtremumPoints.MaxPoint.X.Value - lastPointerPosition.X;
            double dy = HeatingExtremumPoints.MaxPoint.Y.Value - lastPointerPosition.Y;

            double distanceMax = Math.Sqrt(dx * dx + dy * dy);

            double dx1 = HeatingExtremumPoints.MinPoint.X.Value - lastPointerPosition.X;
            double dy1 = HeatingExtremumPoints.MinPoint.Y.Value - lastPointerPosition.Y;

            double distanceMin = Math.Sqrt(dx1 * dx1 + dy1 * dy1);

            if (distanceMax > distanceMin)
            {
                return HeatingExtremumPoints.MinPoint;
            }
            else
            {
                return HeatingExtremumPoints.MaxPoint;
            }
        }

        public bool IsEnabledMovementChart = false;
        public bool IsEnabledMovementVertLines = true;
        public bool IsEnabledMovementPoints = false;

        public LvcPointD LastPointerPosition;

        private bool isDragging = false;

        public void PointerDown(PointerCommandArgs args)
        {
            // при наведении на точку она чуть увеличивается, за нее можно схватиться ЛКМ
            // мб флаг определяющий что конкретная точка схвачена

            isDragging = true;

            var chart = (ICartesianChartView<SkiaSharpDrawingContext>)args.Chart;
            var lastPointerPosition = chart.ScalePixelsToData(args.PointerPosition);

            if (IsEnabledMovementVertLines)
            {
                LastPointerPosition = lastPointerPosition;
                ChangeThumbPosition(LastPointerPosition);
            }

            //if (IsEnabledMovementPoints)
            //{
            //    var dist = FindExtremaNearPointer(lastPointerPosition);
            //}
        }

        public void PointerMove(PointerCommandArgs args)
        {
            if (!isDragging) return;

            var chart = (ICartesianChartView<SkiaSharpDrawingContext>)args.Chart;
            var lastPointerPosition = chart.ScalePixelsToData(args.PointerPosition);

            if (IsEnabledMovementVertLines)
            {
                LastPointerPosition = lastPointerPosition;
                ChangeThumbPosition(LastPointerPosition);
            }
        }

        public void PointerUp(PointerCommandArgs args)
        {
            isDragging = false;
        }

        private void ChangeThumbPosition(LvcPointD lastPointerPosition)
        {
            // update the scroll bar thumb when the user is dragging the chart
            Thumbs[0].Xi = Math.Round(lastPointerPosition.X);
            Thumbs[0].Xj = Math.Round(lastPointerPosition.X);
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
