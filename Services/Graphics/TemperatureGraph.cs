using Avalonia.Controls.Primitives;
using LasAnalyzer.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
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
    public class TemperatureGraph : IGraph
    {
        public ISeries[] ProbeSeries { get; set; }
        public LineSeries<double> LineSeries { get; set; }
        public List<double> Data { get; set; }
        public string Title { get; set; }
        public int WindowSize { get; set; }
        public int CoolingStartIndex { get; set; }
        public TempType TemperatureType { get; set; }

        // базовые индексы нужны чтобы измерить базовые значения показаний зондов
        public int BaseHeatIndex { get; set; }
        public int BaseCoolIndex { get; set; }

        public RectangularSection[] Thumbs { get; set; }

        public TemperatureGraph(string title)
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

        public TemperatureGraph(List<double> data, string title, int windowSize)
        {
            Data = data;
            Title = title;
            WindowSize = windowSize;

            BaseHeatIndex = -1;
            BaseCoolIndex = -1;
            CoolingStartIndex = -1;

            FindHeatingCoolingTransitionIndex();

            FindIndexForBaseValue();

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

        private void FindHeatingCoolingTransitionIndex()
        {
            bool hasHeating = false;
            bool hasCooling = false;
            int coolingStartIndex = -1;

            for (int i = 1; i < Data.Count; i++)
            {
                if (Data[i - 1] < Data[i])
                {
                    hasHeating = true;
                }
                else if (Data[i - 1] > Data[i])
                {
                    hasCooling = true;
                    coolingStartIndex = i;
                }
            }

            if (hasHeating && hasCooling)
            {
                CoolingStartIndex = coolingStartIndex;
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
                // если процесс нагрева или охлаждения не обнаружен
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
            else if (TemperatureType == TempType.Cooling || TemperatureType == TempType.Both)
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

        private int? FindTemperatureRisePoint(List<double> tempData, bool searchLeft)
        {
            int start = searchLeft ? 0 : tempData.Count - 1;
            int step = searchLeft ? 1 : -1;

            for (int i = start; searchLeft ? i < tempData.Count - 1 : i > 0; i += step)
            {
                double tempDifference = searchLeft ? tempData[i + 1] - tempData[i] : tempData[i - 1] - tempData[i];

                if (tempDifference >= 1)
                {
                    return i;
                }
            }

            return null;
        }
    }
}
