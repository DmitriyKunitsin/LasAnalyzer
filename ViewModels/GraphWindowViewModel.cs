using OxyPlot;
using OxyPlot.Axes;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using OxyPlot.Series;
using LasAnalyzer.Services;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.VisualElements;

namespace LasAnalyzer.ViewModels
{
    public class GraphWindowViewModel : ViewModelBase
    {
        private DataGenerator dataGenerator;

        public ISeries[] NearProbeSeries { get; set; }
        public ISeries[] FarProbeSeries { get; set; }
        public ISeries[] FarToNearProbeRatioSeries { get; set; }
        public ISeries[] TemperatureSeries { get; set; }

        public LabelVisual Title { get; set; } =
        new LabelVisual
        {
            Text = "My chart title",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15),
            Paint = new SolidColorPaint(SKColors.DarkSlateGray)
        };

        public GraphWindowViewModel()
        {
            dataGenerator = new DataGenerator();

            var newData = dataGenerator.GenerateGraphData(100);

            NearProbeSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = newData.NearProbe,
                }
            };

            FarProbeSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = newData.FarProbe,
                }
            };

            FarToNearProbeRatioSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = newData.FarToNearProbeRatio,
                }
            };

            TemperatureSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = newData.Temperature,
                }
            };

            UpdateGraphs = ReactiveCommand.CreateFromObservable(UpdateGraphData);
        }

        public ReactiveCommand<Unit, Unit> UpdateGraphs { get; set; }

        private IObservable<Unit> UpdateGraphData()
        {
            var newData = dataGenerator.GenerateGraphData(100);

            var xData = newData.Time; // Ваши данные по оси X
            var yData = newData.NearProbe; // Ваши данные по оси Y

            var updatedSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = yData,
                }
            };

            NearProbeSeries = updatedSeries;

            // Верните Unit для завершения команды
            return Observable.Return(Unit.Default);
        }
    }
}
