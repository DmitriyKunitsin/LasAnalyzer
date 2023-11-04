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
            set 
            {
                this.RaiseAndSetIfChanged(ref thumbs, value); 
            }
        }

        public ReactiveCommand<PointerCommandArgs, Unit> PointerDownCommand { get; }
        public ReactiveCommand<PointerCommandArgs, Unit> PointerMoveCommand { get; }
        public ReactiveCommand<PointerCommandArgs, Unit> PointerUpCommand { get; }

        public bool IsEnabledMovementChart = false;
        public bool IsEnabledMovementVertLines = false;
        public bool IsEnabledMovementPoints = false;

        public LvcPointD LastPointerPosition;

        private bool isDragging = false;

        public GraphService((string, string) titles)
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
                }
            };

            GraphTemperature = new TemperatureGraph(Thumbs, "TEMPER");
            GraphNearProbe = new ProbeGraph(Thumbs, titles.Item1);
            GraphFarProbe = new ProbeGraph(Thumbs, titles.Item2);
            GraphFarToNearProbeRatio = new ProbeGraph(Thumbs, $"{titles.Item2}/{titles.Item1}");
        }

        public GraphService(GraphData graphData, (string, string) titles, int windowSize)
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
                }
            };

            GraphTemperature = new TemperatureGraph(graphData.Temperature, Thumbs, "TEMPER", windowSize);

            CoolingStartIndex = GraphTemperature.CoolingStartIndex;
            TemperatureType = GraphTemperature.TemperatureType;

            var baseHeatIndex = GraphTemperature.BaseHeatIndex;
            var baseCoolIndex = GraphTemperature.BaseCoolIndex;

            GraphNearProbe = new ProbeGraph(graphData.NearProbe, Thumbs, titles.Item1, CoolingStartIndex, baseHeatIndex, baseCoolIndex);
            GraphFarProbe = new ProbeGraph(graphData.FarProbe, Thumbs, titles.Item2, CoolingStartIndex, baseHeatIndex, baseCoolIndex);
            GraphFarToNearProbeRatio = new ProbeGraph(graphData.FarToNearProbeRatio, Thumbs, $"{titles.Item2}/{titles.Item1}", CoolingStartIndex, baseHeatIndex, baseCoolIndex);

            PointerDownCommand = ReactiveCommand.Create<PointerCommandArgs>(PointerDown);
            PointerMoveCommand = ReactiveCommand.Create<PointerCommandArgs>(PointerMove);
            PointerUpCommand = ReactiveCommand.Create<PointerCommandArgs>(PointerUp);
        }

        private void PointerDown(PointerCommandArgs args)
        {
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

        private void PointerMove(PointerCommandArgs args)
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

        private void PointerUp(PointerCommandArgs args)
        {
            isDragging = false;
        }

        private double FindExtremaNearPointer(LvcPointD LastPointerPosition)
        {
            List<ProbeGraph> DistList = new List<ProbeGraph>
            {
                GraphNearProbe,
                GraphFarProbe,
                GraphFarToNearProbeRatio
            };

            var asd = DistList.Select(graph => graph.GetNearlyExtrema(LastPointerPosition)).OrderBy(point => point);

            return 0;
        }
        
        private void ChangeThumbPosition(LvcPointD lastPointerPosition)
        {
            // update the scroll bar thumb when the user is dragging the chart
            GraphNearProbe.Thumbs[0].Xi = Math.Round(lastPointerPosition.X);
            GraphNearProbe.Thumbs[0].Xj = Math.Round(lastPointerPosition.X);

            GraphFarProbe.Thumbs[0].Xi = Math.Round(lastPointerPosition.X);
            GraphFarProbe.Thumbs[0].Xj = Math.Round(lastPointerPosition.X);

            GraphFarToNearProbeRatio.Thumbs[0].Xi = Math.Round(lastPointerPosition.X);
            GraphFarToNearProbeRatio.Thumbs[0].Xj = Math.Round(lastPointerPosition.X);

            GraphTemperature.Thumbs[0].Xi = Math.Round(lastPointerPosition.X);
            GraphTemperature.Thumbs[0].Xj = Math.Round(lastPointerPosition.X);
        }
    }
}
