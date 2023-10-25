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
        private PlotModel nearProbeModel;
        private PlotModel farProbeModel;
        private PlotModel farToNearProbeRatioModel;
        private PlotModel temperatureModel;

        private DataGenerator dataGenerator;

        public ISeries[] Series { get; set; }

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
            InitializeGraphs();

            Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = dataGenerator.GenerateGraphData(100).NearProbe,
                    Fill = null
                }
            };
        }

        public PlotModel NearProbeModel
        {
            get => nearProbeModel;
            set => this.RaiseAndSetIfChanged(ref nearProbeModel, value);
        }

        public PlotModel FarProbeModel
        {
            get => farProbeModel;
            set => this.RaiseAndSetIfChanged(ref farProbeModel, value);
        }

        public PlotModel FarToNearProbeRatioModel
        {
            get => farToNearProbeRatioModel;
            set => this.RaiseAndSetIfChanged(ref farToNearProbeRatioModel, value);
        }

        public PlotModel TemperatureModel
        {
            get => temperatureModel;
            set => this.RaiseAndSetIfChanged(ref temperatureModel, value);
        }

        public ReactiveCommand<Unit, Unit> UpdateGraphs { get; set; }

        private void InitializeGraphs()
        {
            dataGenerator = new DataGenerator();

            // Инициализация моделей графиков и настройки по умолчанию
            NearProbeModel = CreateGraphModel("Near Probe", "X-Axis Title", "Y-Axis Title");
            FarProbeModel = CreateGraphModel("Far Probe", "X-Axis Title", "Y-Axis Title");
            FarToNearProbeRatioModel = CreateGraphModel("Far/Near Ratio", "X-Axis Title", "Y-Axis Title");
            TemperatureModel = CreateGraphModel("Temperature", "X-Axis Title", "Y-Axis Title");

            // Привязка команды для обновления данных графиков
            UpdateGraphs = ReactiveCommand.CreateFromObservable(UpdateGraphData);
        }

        private PlotModel CreateGraphModel(string title, string xTitle, string yTitle)
        {
            var model = new PlotModel { Title = title };
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = xTitle });
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = yTitle });

            return model;
        }

        private IObservable<Unit> UpdateGraphData()
        {
            var newData = dataGenerator.GenerateGraphData(100);

            var xData = newData.Time; // Ваши данные по оси X
            var yData = newData.NearProbe; // Ваши данные по оси Y

            var newNearProbeModel = CreateGraphModel("Near Probe", "X-Axis Title", "Y-Axis Title");
            var series = new LineSeries();
            for (int i = 0; i < xData.Count; i++)
            {
                series.Points.Add(new DataPoint(xData[i], yData[i]));
            }
            newNearProbeModel.Series.Add(series);

            NearProbeModel = newNearProbeModel;

            // Верните Unit для завершения команды
            return Observable.Return(Unit.Default);
        }
    }
}
