using LasAnalyzer.Services;
using ReactiveUI;
using System.Reactive.Subjects;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System;
using LasAnalyzer.Models;
using LasAnalyzer.Views;
using System.Linq;
using LiveChartsCore.Geo;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.VisualElements;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView.SKCharts;
using System.Drawing.Imaging;

namespace LasAnalyzer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private LasFileReader _lasFileReader;
        private DocxWriter _docxWriter;

        private BehaviorSubject<GraphData> _lasDataSubject = new BehaviorSubject<GraphData>(null);

        private SeriesData _seriesData;

        public GraphData LasData
        {
            get => _lasDataSubject.Value;
        }

        public SeriesData SeriesData
        {
            get => _seriesData;
            set => this.RaiseAndSetIfChanged(ref _seriesData, value);
        }
        public LabelVisual Title { get; set; } =
        new LabelVisual
        {
            Text = "My chart title",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15),
            Paint = new SolidColorPaint(SKColors.DarkSlateGray)
        };

        public ReactiveCommand<Unit, Unit> OpenLasFileCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenGraphWindowCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateAndSaveReportCommand { get; }

        public MainWindowViewModel()
        {
            _lasFileReader = new LasFileReader();
            _docxWriter = new DocxWriter();

            // Подписка на изменения в BehaviorSubject и привязка к свойству LasData
            _lasDataSubject
                .ToProperty(this, x => x.LasData);

            OpenLasFileCommand = ReactiveCommand.CreateFromTask(OpenLasFileAsync);
            OpenGraphWindowCommand = ReactiveCommand.Create(OpenGraphWindow);
            CreateAndSaveReportCommand = ReactiveCommand.Create(CreateAndSaveReport);

            MessageBus.Current.Listen<GraphData>("GraphDataMessage")
            .Subscribe(graphData => ReceiveGraphData(graphData));

            SeriesData = new SeriesData();
        }

        private void ReceiveGraphData(GraphData graphData)
        {
            _lasDataSubject.OnNext(graphData);
        }

        
        private void CreateAndSaveReport()
        {
            var tempType = getTempType();
            var windowSize = getWindowSize();
            if (LasDataForGamma is not null)
            {
                SaveReport(LasDataForGamma, "RSD", "RLD");
            }
            if (LasDataForNeutronic is not null)
            {
                SaveReport(LasDataForNeutronic, "NTNC", "FTNC");
            }
        }

        private void SaveReport(GraphData lasData, string nearProbeTitle, string farProbeTitle)
        {
            List<byte[]> chartImageDatas = new List<byte[]>()
                {
                    createChartImage(lasData.NearProbe, nearProbeTitle),
                    createChartImage(lasData.FarProbe, farProbeTitle),
                    createChartImage(lasData.FarToNearProbeRatio, $"{nearProbeTitle}/{farProbeTitle}"),
                    createChartImage(lasData.Temperature, "TEMPER")
                };

            // todo: complete this
            ReportModel ReportModel = new ReportModel()
            {
                SerialNumber = "12312312",
                DeviceType = "gg nn",
                TestDate = "11.22.33",
                NearProbeThreshold = 0,
                FarProbeThreshold = 0,
                Graphs = chartImageDatas,
                Results = CalculatorWrapper(lasData),
                Conclusion = "> < 5 %"
            };
            _docxWriter.CreateReport(ReportModel, Directory.GetCurrentDirectory() + "\\out.docx");
        }

        private (List<Result>, List<Result>) CalculatorWrapper(GraphData lasData)
        {
            var calculator = new Calculator();
            var tableList = new List<List<Result>>();

            if (processHeatingControl.isChecked)
                tableList.Add(calculator.CalculateMetrics(lasData, TempType.Heating, windowSize));

            if (processCoolingControl.isChecked)
                tableList.Add(calculator.CalculateMetrics(lasData, TempType.Cooling, windowSize));

            return tableList;
        }

        private byte[] createChartImage(List<double> data, string title)
        {
            var cartesianChart = new SKCartesianChart
            {
                Width = 1000,
                Height = 400,
                Series = new ISeries[]
                {
                    new LineSeries<double> { Values = data },
                },
                Title = new LabelVisual
                {
                    Text = title,
                    TextSize = 30,
                    Padding = new Padding(15),
                    Paint = new SolidColorPaint(0xff303030)
                },
                LegendPosition = LiveChartsCore.Measure.LegendPosition.Right,
                Background = SKColors.White
            };

            return cartesianChart.GetImage().Encode().ToArray();
        }

        private void OpenGraphWindow()
        {
            var graphWindow = ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Windows.OfType<GraphWindow>().FirstOrDefault();

            if (graphWindow == null)
            {
                graphWindow = new GraphWindow();
                MessageBus.Current.SendMessage(LasData, "GraphDataMessage");
                graphWindow.Show();
            }
            else
            {
                graphWindow.Activate();
            }
        }

        private async Task<Unit> OpenLasFileAsync()
        {
            var file = await DoOpenFilePickerAsync();
            if (file is null) return Unit.Default;

            var lasData = _lasFileReader.OpenLasFile(file.Path.AbsolutePath);
            if (lasData is not null)
            {
                _lasDataSubject.OnNext(lasData);

                SeriesData = new SeriesData(lasData);
            }

            return Unit.Default;
        }

        private async Task<IStorageFile?> DoOpenFilePickerAsync()
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
                desktop.MainWindow?.StorageProvider is not { } provider)
                throw new NullReferenceException("Missing StorageProvider instance.");

            FilePickerFileType LasFileType = new("Las files")
            {
                Patterns = new[] { "*.las" },
            };

            var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "Open Text File",
                FileTypeFilter = new[] { LasFileType },
                AllowMultiple = false
            });

            return files?.Count >= 1 ? files[0] : null;
        }
    }
}