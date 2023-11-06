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
using LiveChartsCore.Defaults;

namespace LasAnalyzer.Services.Graphics
{
    public class GraphService
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

        public RectangularSection[] Thumbs { get; set; }

        public ReactiveCommand<PointerCommandArgs, Unit> PointerDownCommand { get; }
        public ReactiveCommand<PointerCommandArgs, Unit> PointerMoveCommand { get; }
        public ReactiveCommand<PointerCommandArgs, Unit> PointerUpCommand { get; }

        public bool IsEnabledMovementVertLines { get; set; } = false;
        public bool IsEnabledMovementPoints { get; set; } = false;

        public LvcPointD LastPointerPosition { get; set; }
        public ObservablePoint NearlyExtrema { get; set; }

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

            GraphTemperature = new TemperatureGraph("TEMPER");
            GraphNearProbe = new ProbeGraph(titles.Item1);
            GraphFarProbe = new ProbeGraph(titles.Item2);
            GraphFarToNearProbeRatio = new ProbeGraph($"{titles.Item2}/{titles.Item1}");
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

            GraphTemperature = new TemperatureGraph(graphData.Temperature, "TEMPER", windowSize);

            CoolingStartIndex = GraphTemperature.CoolingStartIndex;
            TemperatureType = GraphTemperature.TemperatureType;

            var baseHeatIndex = GraphTemperature.BaseHeatIndex;
            var baseCoolIndex = GraphTemperature.BaseCoolIndex;

            GraphNearProbe = new ProbeGraph(graphData.NearProbe, titles.Item1, CoolingStartIndex, baseHeatIndex, baseCoolIndex);
            GraphFarProbe = new ProbeGraph(graphData.FarProbe, titles.Item2, CoolingStartIndex, baseHeatIndex, baseCoolIndex);
            GraphFarToNearProbeRatio = new ProbeGraph(graphData.FarToNearProbeRatio, $"{titles.Item2}/{titles.Item1}", CoolingStartIndex, baseHeatIndex, baseCoolIndex);

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

            if (IsEnabledMovementPoints)
            {
                if (chart.Series.Count() > 1)
                {
                    NearlyExtrema = ((ScatterSeries<ObservablePoint>)chart.Series.ToList()[1]).Values.Skip(2)
                        .Where(point => point != null)
                        .OrderBy(point => GetDistanceToPointer(point, lastPointerPosition)).First();
                    var x = Math.Round(lastPointerPosition.X) > ((LineSeries<double>)chart.Series.ToList()[0]).Values.Count() - 1 
                        ?
                        ((LineSeries<double>)chart.Series.ToList()[0]).Values.Count() - 1 
                        :
                        Math.Round(lastPointerPosition.X);
                    x = x < 0 ? 0 : x;
                    NearlyExtrema.X = x;
                    NearlyExtrema.Y = ((LineSeries<double>)chart.Series.ToList()[0]).Values.ToList()[Convert.ToInt32(x)];
                }
            }
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

            if (IsEnabledMovementPoints)
            {
                if (chart.Series.Count() > 1)
                {
                    var x = Math.Round(lastPointerPosition.X) > ((LineSeries<double>)chart.Series.ToList()[0]).Values.Count() - 1 
                        ?
                        ((LineSeries<double>)chart.Series.ToList()[0]).Values.Count() - 1 
                        :
                        Math.Round(lastPointerPosition.X);
                    x = x < 0 ? 0 : x;
                    NearlyExtrema.X = x;
                    NearlyExtrema.Y = ((LineSeries<double>)chart.Series.ToList()[0]).Values.ToList()[Convert.ToInt32(x)];
                }
            }
        }

        private void PointerUp(PointerCommandArgs args)
        {
            isDragging = false;
        }

        private double GetDistanceToPointer(ObservablePoint point, LvcPointD lastPointerPosition)
        {
            double dx = Math.Abs(point.X.Value - lastPointerPosition.X);

            return dx;
        }

        private ProbeGraph FindExtremaNearPointer(LvcPointD lastPointerPosition)
        {
            List<ProbeGraph> pointList = new List<ProbeGraph>
            {
                GraphNearProbe,
                GraphFarProbe,
                GraphFarToNearProbeRatio
            };

            var extrema = pointList.OrderBy(graph => GetDistanceToPointer(graph.GetNearlyExtrema(lastPointerPosition), lastPointerPosition)).First();

            return extrema;
        }
        
        private void ChangeThumbPosition(LvcPointD lastPointerPosition)
        {
            // update the scroll bar thumb when the user is dragging the chart
            GraphTemperature.ChangeThumbPosition(lastPointerPosition);
            GraphNearProbe.ChangeThumbPosition(lastPointerPosition);
            GraphFarProbe.ChangeThumbPosition(lastPointerPosition);
            GraphFarToNearProbeRatio.ChangeThumbPosition(lastPointerPosition);
        }
    }
}
