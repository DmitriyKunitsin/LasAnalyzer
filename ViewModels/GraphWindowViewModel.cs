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
using LasAnalyzer.Models;
using LiveChartsCore.Geo;

namespace LasAnalyzer.ViewModels
{
    public class GraphWindowViewModel : ViewModelBase
    {
        private GraphData _lasData;

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

        public GraphData LasData
        {
            get => _lasData;
            set => this.RaiseAndSetIfChanged(ref _lasData, value);
        }

        public event EventHandler DataUpdated;

        public ReactiveCommand<Unit, Unit> UpdateGraphs { get; set; }


        public GraphWindowViewModel()
        {
            MessageBus.Current.SendMessage(LasData, "GraphDataMessage");

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

            UpdateGraphs = ReactiveCommand.CreateFromObservable(UpdateGraphData);
        }

        

        private IObservable<Unit> UpdateGraphData()
        {
            NearProbeSeries = new ISeries[]
            {
                new LineSeries<double> { Values = _lasData.NearProbe }
            };

            FarProbeSeries = new ISeries[]
            {
                new LineSeries<double> { Values = _lasData.FarProbe }
            };

            FarToNearProbeRatioSeries = new ISeries[]
            {
                new LineSeries<double> { Values = _lasData.FarToNearProbeRatio }
            };

            TemperatureSeries = new ISeries[]
            {
                new LineSeries<double> { Values = _lasData.Temperature }
            };

            ///
            //var xData = _lasData.Time; // Ваши данные по оси X
            //var yData = _lasData.NearProbe; // Ваши данные по оси Y

            //var updatedSeries = new ISeries[]
            //{
            //    new LineSeries<double>
            //    {
            //        Values = yData,
            //    }
            //};

            //NearProbeSeries = updatedSeries;

            // Верните Unit для завершения команды
            return Observable.Return(Unit.Default);
        }
    }
}
