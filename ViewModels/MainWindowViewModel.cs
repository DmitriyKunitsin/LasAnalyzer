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
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using Avalonia.Input;
using System.Reflection;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.Measure;

namespace LasAnalyzer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private LasFileReader _lasFileReader;
        private DocxWriter _docxWriter;

        private SeriesData _seriesDataForGamma;
        private SeriesData _seriesDataForNeutronic;

        private int windowSize;
        private int smoothingIterations;
        private bool isHeatingSelected;
        private bool isCoolingSelected;
        private bool isGammaSelected;
        private bool isNeutronicSelected;

        public GraphData LasDataForGamma { get; set; }
        public GraphData LasDataForNeutronic { get; set; }

        public SeriesData SeriesDataForGamma
        {
            get => _seriesDataForGamma;
            set => this.RaiseAndSetIfChanged(ref _seriesDataForGamma, value);
        }

        public SeriesData SeriesDataForNeutronic
        {
            get => _seriesDataForNeutronic;
            set => this.RaiseAndSetIfChanged(ref _seriesDataForNeutronic, value);
        }

        //public Axis[] XAxes { get; set; } =
        //{
        //    new Axis
        //    {
        //        CrosshairLabelsBackground = SKColors.DarkOrange.AsLvcColor(),
        //        CrosshairLabelsPaint = new SolidColorPaint(SKColors.DarkRed, 1),
        //        CrosshairPaint = new SolidColorPaint(SKColors.DarkOrange, 1),
        //        //Labeler = value => value.ToString("N2"),
        //        CrosshairSnapEnabled = true
        //    }
        //};

        public int WindowSize
        {
            get => windowSize;
            set => this.RaiseAndSetIfChanged(ref windowSize, value);
        }

        public int SmoothingIterations
        {
            get => smoothingIterations;
            set => this.RaiseAndSetIfChanged(ref smoothingIterations, value);
        }

        public bool IsHeatingSelected
        {
            get => isHeatingSelected;
            set => this.RaiseAndSetIfChanged(ref isHeatingSelected, value);
        }

        public bool IsCoolingSelected
        {
            get => isCoolingSelected;
            set => this.RaiseAndSetIfChanged(ref isCoolingSelected, value);
        }

        public bool IsGammaSelected
        {
            get => isGammaSelected;
            set => this.RaiseAndSetIfChanged(ref isGammaSelected, value);
        }

        public bool IsNeutronicSelected
        {
            get => isNeutronicSelected;
            set => this.RaiseAndSetIfChanged(ref isNeutronicSelected, value);
        }

        public LabelVisual Title { get; set; } =
        new LabelVisual
        {
            Text = "My chart title",
            TextSize = 25,
            Padding = new Padding(15),
            Paint = new SolidColorPaint(SKColors.DarkSlateGray)
        };

        private LineGeometry line;
        public LineGeometry Line
        {
            get => line;
            set => this.RaiseAndSetIfChanged(ref line, value);
        }

        private double _xSection;
        private double _ySection;
        public double XSection
        {
            get => _xSection;
            set => this.RaiseAndSetIfChanged(ref _xSection, value);
        }

        public double YSection
        {
            get => _ySection;
            set => this.RaiseAndSetIfChanged(ref _ySection, value);
        }

        public ReactiveCommand<Unit, Unit> OpenLasFileCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenGraphWindowCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateAndSaveReportCommand { get; }

        public ReactiveCommand<PointerCommandArgs, Unit> PointerDownCommand { get; }
        public ReactiveCommand<PointerCommandArgs, Unit> PointerMoveCommand { get; }
        public ReactiveCommand<PointerCommandArgs, Unit> PointerUpCommand { get; }

        public MainWindowViewModel()
        {
            _lasFileReader = new LasFileReader();
            _docxWriter = new DocxWriter();

            OpenLasFileCommand = ReactiveCommand.CreateFromTask(OpenLasFileAsync);
            OpenGraphWindowCommand = ReactiveCommand.Create(OpenGraphWindow);
            CreateAndSaveReportCommand = ReactiveCommand.Create(CreateAndSaveReport);

            MessageBus.Current.Listen<GraphData>("GraphDataMessage")
            .Subscribe(graphData => ReceiveGraphData(graphData)); ///

            SeriesDataForGamma = new SeriesData();
            SeriesDataForNeutronic = new SeriesData();

            PointerDownCommand = ReactiveCommand.Create<PointerCommandArgs>(PointerDown);
            PointerMoveCommand = ReactiveCommand.Create<PointerCommandArgs>(PointerMove);
            PointerUpCommand = ReactiveCommand.Create<PointerCommandArgs>(PointerUp);

            InvisibleX = new[] { new Axis { IsVisible = true } };
            YAxis = new[] 
            { new Axis
                {
                }
            };
            ScrollableAxes = new[] { new Axis() };

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


            WindowSize = 60;
            SmoothingIterations = 3;
            IsHeatingSelected = true;
            IsCoolingSelected = true;
            IsGammaSelected = true;
        }
        private Axis[] invisibleX;
        private Axis[] invisibleY;
        private Axis[] scrollableAxes;
        private RectangularSection[] thumbs;

        public Axis[] InvisibleX
        {
            get => invisibleX;
            set => this.RaiseAndSetIfChanged(ref invisibleX, value);
        }
        public Axis[] YAxis
        {
            get => invisibleY;
            set => this.RaiseAndSetIfChanged(ref invisibleY, value);
        }
        public RectangularSection[] Thumbs
        {
            get => thumbs;
            set => this.RaiseAndSetIfChanged(ref thumbs, value);
        }

        public Axis[] ScrollableAxes
        {
            get => scrollableAxes;
            set => this.RaiseAndSetIfChanged(ref scrollableAxes, value);
        }

        private bool isDragging = false;
        private LvcPointD lastPointerPosition;

        private void PointerDown(PointerCommandArgs args)
        {
            isDragging = true;
            var chart = (ICartesianChartView<SkiaSharpDrawingContext>)args.Chart;
            lastPointerPosition = chart.ScalePixelsToData(args.PointerPosition);
            var thumb = Thumbs[0];

            // update the scroll bar thumb when the user is dragging the chart
            thumb.Xi = lastPointerPosition.X;
            thumb.Xj = lastPointerPosition.X;
        }

        private void PointerMove(PointerCommandArgs args)
        {
            if (!isDragging) return;

            var chart = (ICartesianChartView<SkiaSharpDrawingContext>)args.Chart;
            var lastPointerPosition = chart.ScalePixelsToData(args.PointerPosition);

            var thumb = Thumbs[0];

            // update the scroll bar thumb when the user is dragging the chart
            thumb.Xi = lastPointerPosition.X;
            thumb.Xj = lastPointerPosition.X;

            // update the chart visible range
            //ScrollableAxes[0].MinLimit = thumb.Xi;
            //ScrollableAxes[0].MaxLimit = thumb.Xj;
        }

        private void PointerUp(PointerCommandArgs args)
        {
            isDragging = false;
        }

        private void ReceiveGraphData(GraphData graphData)
        {
            LasDataForGamma = graphData; ///
        }
        
        private void CreateAndSaveReport()
        {
            _docxWriter.CreateAndSaveReport(
                LasDataForGamma,
                LasDataForNeutronic,
                IsHeatingSelected,
                IsCoolingSelected,
                WindowSize);
        }

        private void OpenGraphWindow()
        {
            var graphWindow = ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Windows.OfType<GraphWindow>().FirstOrDefault();

            if (graphWindow == null)
            {
                graphWindow = new GraphWindow();
                MessageBus.Current.SendMessage(LasDataForGamma, "GraphDataMessage");
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
            if (lasData.Item1 is not null)
            {
                LasDataForGamma = lasData.Item1;
                LasDataForNeutronic = lasData.Item2;

                SeriesDataForGamma = new SeriesData(lasData.Item1);
                SeriesDataForNeutronic = new SeriesData(lasData.Item2);

                YAxis = new[]
                {
                    new Axis
                    {
                        //MaxLimit = LasDataForGamma.NearProbe.Max() * 1.1,
                        MinLimit = LasDataForGamma.NearProbe.Min() * 0.9
                    }
                };
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