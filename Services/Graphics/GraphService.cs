using LasAnalyzer.Models;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.Kernel.Events;
using System.Reactive;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.Drawing;

namespace LasAnalyzer.Services.Graphics
{
    public class GraphService : ReactiveObject
    {
        // при инициализации будут создаваться 4 объекта Graph
        // которые и будут представлять графики
        // в этом классе будет определятья точка когда заканчивается нагрев и начинается охлаждение

        public ProbeGraph GraphNearProbe { get; set; }
        public ProbeGraph GraphFarProbe { get; set; }
        public ProbeGraph GraphFarToNearProbeRatio { get; set; }
        public TemperatureGraph GraphTemperature { get; set; }
        public TempType TemperatureType { get; set; }
        public int CoolingStartIndex { get; set; }

        private RectangularSection[] thumbs;
        public RectangularSection[] Thumbs
        {
            get => thumbs;
            set => this.RaiseAndSetIfChanged(ref thumbs, value);
        }

        public ReactiveCommand<PointerCommandArgs, Unit> PointerDownCommand { get; }
        public ReactiveCommand<PointerCommandArgs, Unit> PointerMoveCommand { get; }
        public ReactiveCommand<PointerCommandArgs, Unit> PointerUpCommand { get; }

        private bool isDragging = false;
        private LvcPointD lastPointerPosition;

        public GraphService((string, string) titles)
        {
            GraphTemperature = new TemperatureGraph("TEMPER");
            GraphNearProbe = new ProbeGraph(titles.Item1);
            GraphFarProbe = new ProbeGraph(titles.Item2);
            GraphFarToNearProbeRatio = new ProbeGraph($"{titles.Item2}/{titles.Item1}");

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

        public GraphService(GraphData graphData, (string, string) titles, int windowSize)
        {
            GraphTemperature = new TemperatureGraph(graphData.Temperature, "TEMPER", windowSize);

            CoolingStartIndex = GraphTemperature.CoolingStartIndex;
            TemperatureType = GraphTemperature.TemperatureType;

            var baseHeatIndex = GraphTemperature.BaseHeatIndex;
            var baseCoolIndex = GraphTemperature.BaseCoolIndex;

            GraphNearProbe = new ProbeGraph(graphData.NearProbe, titles.Item1, CoolingStartIndex, baseHeatIndex, baseCoolIndex);
            GraphFarProbe = new ProbeGraph(graphData.FarProbe, titles.Item2, CoolingStartIndex, baseHeatIndex, baseCoolIndex);
            GraphFarToNearProbeRatio = new ProbeGraph(graphData.FarToNearProbeRatio, $"{titles.Item2}/{titles.Item1}", CoolingStartIndex, baseHeatIndex, baseCoolIndex);

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

            PointerDownCommand = ReactiveCommand.Create<PointerCommandArgs>(PointerDown);
            PointerMoveCommand = ReactiveCommand.Create<PointerCommandArgs>(PointerMove);
            PointerUpCommand = ReactiveCommand.Create<PointerCommandArgs>(PointerUp);
        }

        private void PointerDown(PointerCommandArgs args)
        {
            isDragging = true;

            var chart = (ICartesianChartView<SkiaSharpDrawingContext>)args.Chart;
            lastPointerPosition = chart.ScalePixelsToData(args.PointerPosition);

            ChangeThumbPosition(GraphNearProbe.Thumbs[0], lastPointerPosition);
            ChangeThumbPosition(GraphFarProbe.Thumbs[0], lastPointerPosition);
            ChangeThumbPosition(GraphFarToNearProbeRatio.Thumbs[0], lastPointerPosition);
            ChangeThumbPosition(GraphTemperature.Thumbs[0], lastPointerPosition);
        }

        private void PointerMove(PointerCommandArgs args)
        {
            if (!isDragging) return;

            var chart = (ICartesianChartView<SkiaSharpDrawingContext>)args.Chart;
            lastPointerPosition = chart.ScalePixelsToData(args.PointerPosition);

            ChangeThumbPosition(GraphNearProbe.Thumbs[0], lastPointerPosition);
            ChangeThumbPosition(GraphFarProbe.Thumbs[0], lastPointerPosition);
            ChangeThumbPosition(GraphFarToNearProbeRatio.Thumbs[0], lastPointerPosition);
            ChangeThumbPosition(GraphTemperature.Thumbs[0], lastPointerPosition);
        }

        private void PointerUp(PointerCommandArgs args)
        {
            isDragging = false;
        }

        private void ChangeThumbPosition(RectangularSection thumb, LvcPointD lastPointerPosition)
        {
            // update the scroll bar thumb when the user is dragging the chart
            thumb.Xi = Math.Round(lastPointerPosition.X);
            thumb.Xj = Math.Round(lastPointerPosition.X);
        }
    }
}
