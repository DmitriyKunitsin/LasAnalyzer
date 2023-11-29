using Avalonia.Controls.Primitives;
using LasAnalyzer.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ReactiveUI;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace LasAnalyzer.Services.Graphics
{
    public class TemperatureGraph : ReactiveObject, IGraph
    {
        private ISeries[] _probeSeries;
        public ISeries[] ProbeSeries
        {
            get => _probeSeries;
            set => this.RaiseAndSetIfChanged(ref _probeSeries, value);
        }

        private Axis[] yAxis;
        public Axis[] YAxis
        {
            get => yAxis;
            set => this.RaiseAndSetIfChanged(ref yAxis, value);
        }
        private Axis[] xAxis;
        public Axis[] XAxis
        {
            get => xAxis;
            set => this.RaiseAndSetIfChanged(ref xAxis, value);
        }
        public LineSeries<double?> LineSeries { get; set; }
        public List<double?> Data { get; set; }
        public string Title { get; set; }
        public int WindowSize { get; set; }
        public int TransitionIndex { get; set; }
        public TempType TemperatureType { get; set; }

        // базовые индексы нужны чтобы измерить базовые значения показаний зондов
        public int BaseHeatIndex { get; set; }
        public int BaseCoolIndex { get; set; }

        public RectangularSection[] Thumbs { get; set; }

        public TemperatureGraph(string title)
        {
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
                },
                new RectangularSection
                {
                    Fill = new SolidColorPaint(new SKColor(255, 205, 210, 100)),
                    Xi = 0,
                    Xj = 0,
                    Stroke = new SolidColorPaint
                    {
                        Color = SKColors.DeepSkyBlue,
                        StrokeThickness = 3,
                        ZIndex = 2
                    }
                }
            };

            Title = title;

            YAxis = new[]
            {
                new Axis()
            };

            XAxis = new[]
            {
                new Axis()
            };

            LineSeries = new LineSeries<double?>();

            ProbeSeries = new ISeries[]
            {
                LineSeries,
            };
        }

        public TemperatureGraph(List<double?> data, string title, int windowSize)
        {
            Data = data;
            Title = title;
            WindowSize = windowSize;

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
                },
                new RectangularSection
                {
                    Fill = new SolidColorPaint(new SKColor(255, 205, 210, 100)),
                    Xi = data.Count - 1,
                    Xj = data.Count - 1,
                    Stroke = new SolidColorPaint
                    {
                        Color = SKColors.DeepSkyBlue,
                        StrokeThickness = 3,
                        ZIndex = 2
                    }
                }
            };

            SetProbeSeriesData();
        }

        private void SetProbeSeriesData()
        {
            BaseHeatIndex = -1;
            BaseCoolIndex = -1;
            TransitionIndex = -1;

            FindHeatingCoolingTransitionIndex();

            FindIndexForBaseValue();

            Thumbs[0].Xi = 0;
            Thumbs[0].Xj = 0;

            Thumbs[1].Xi = Data.Count - 1;
            Thumbs[1].Xj = Data.Count - 1;

            var solidColorPaintFat = new SolidColorPaint
            {
                Color = SKColors.Black,
                StrokeThickness = 2,
            };
            var solidColorPaintSlim = new SolidColorPaint
            {
                Color = SKColors.Black,
                StrokeThickness = 0.5f,
            };

            YAxis = new[]
            {
                new Axis
                {
                    Name = Title,
                    MaxLimit = Data.Max() * 1.1,
                    MinLimit = Data.Min() * 0.9,

                    SeparatorsPaint = solidColorPaintFat,
                    SubseparatorsPaint = solidColorPaintSlim,
                    SubseparatorsCount = 5,
                    TicksPaint = solidColorPaintFat,
                    SubticksPaint = solidColorPaintSlim
                }
            };

            XAxis = new[]
            {
                new Axis
                {
                    SeparatorsPaint = solidColorPaintFat,
                    SubseparatorsPaint = solidColorPaintSlim,
                    SubseparatorsCount = 5,
                    TicksPaint = solidColorPaintFat,
                    SubticksPaint = solidColorPaintSlim
                }
            };

            LineSeries = new LineSeries<double?>
            {
                Values = Data,
                GeometryStroke = null,
                GeometryFill = null,
                Fill = null,
                Stroke = new SolidColorPaint
                {
                    Color = SKColors.RoyalBlue,
                    StrokeThickness = 3,
                    ZIndex = 1
                },
                LineSmoothness = 0,
                ZIndex = 1,
            };

            ProbeSeries = new ISeries[]
            {
                LineSeries,
            };
        }

        public void PointerDown(PointerCommandArgs args)
        {
            throw new NotImplementedException();
        }

        public void PointerMove(PointerCommandArgs args)
        {
            throw new NotImplementedException();
        }

        public void PointerUp(PointerCommandArgs args)
        {
            throw new NotImplementedException();
        }

        public void ChangeThumbPosition(LvcPointD lastPointerPosition)
        {
            // update the scroll bar thumb when the user is dragging the chart
            var numVertLine = 0;
            if (Math.Abs(Thumbs[0].Xi.Value - lastPointerPosition.X) > Math.Abs(Thumbs[1].Xi.Value - lastPointerPosition.X))
            {
                numVertLine = 1;
            }
            Thumbs[numVertLine].Xi = lastPointerPosition.X;
            Thumbs[numVertLine].Xj = lastPointerPosition.X;
        }

        public void CropData()
        {
            Data = Data.Skip(Convert.ToInt32(Thumbs[0].Xi.Value)).ToList();
            Data = Data.Take(Convert.ToInt32(Thumbs[1].Xi.Value)).ToList();

            SetProbeSeriesData();
        }

        private void FindHeatingCoolingTransitionIndex()
        {
            bool hasHeating = false;
            bool hasCooling = false;
            int argMax = -1;
            double? max = Data.Max();
            double? threshold = 2;

            for (int i = Data.Count - 1; i > 0; i--)
            {
                // нахождение последнего максимума
                // обработка дребезга (+-0,5)
                // продумать для 3 случаев, только нагрев или охлад или оба

                if (Data[i] == max && argMax == -1)
                {
                    argMax = i;
                }
                if (argMax == -1 && max - Data[i] >= threshold)
                {
                    hasCooling = true;
                }
                if (argMax != -1 && max - Data[i] >= threshold)
                {
                    hasHeating = true;
                }
            }

            if (hasHeating && hasCooling)
            {
                TransitionIndex = argMax;
                TemperatureType = TempType.Both;
            }
            else if (hasHeating)
            {
                TemperatureType = TempType.Heating;
            }
            else if (hasCooling)
            {
                TemperatureType = TempType.Cooling;
            }
            else
            {
                // Если процесс нагрева или охлаждения не обнаружен
                TemperatureType = TempType.Both;
            }
        }

        private void FindIndexForBaseValue()
        {
            if (TemperatureType == TempType.Heating || TemperatureType == TempType.Both)
            {
                var tBaseMaxIndex = FindTemperatureRisePoint(Data, searchLeft: true);
                if (tBaseMaxIndex != null)
                {
                    BaseHeatIndex = Math.Min(tBaseMaxIndex.Value, WindowSize * 5);
                }
            }
            if (TemperatureType == TempType.Cooling || TemperatureType == TempType.Both)
            {
                var tBaseMaxIndex = FindTemperatureRisePoint(Data, searchLeft: false);
                if (tBaseMaxIndex != null)
                {
                    if ((Data.Count - 1) - tBaseMaxIndex < WindowSize * 5)
                    {
                        BaseCoolIndex = tBaseMaxIndex.Value;
                    }
                    else
                    {
                        BaseCoolIndex = (Data.Count - 1) - WindowSize * 5;
                    }
                }
            }
        }

        private int? FindTemperatureRisePoint(List<double?> tempData, bool searchLeft)
        {
            int start = searchLeft ? 0 : tempData.Count - 1;
            int step = searchLeft ? 1 : -1;

            for (int i = start; searchLeft ? i < tempData.Count - 1 : i > 0; i += step)
            {
                double tempDifference = searchLeft ? tempData[i + 1].Value - tempData[i].Value : tempData[i - 1].Value - tempData[i].Value;

                if (tempDifference >= 1)
                {
                    return i;
                }
            }

            return null;
        }
    }
}
