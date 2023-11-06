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
using LasAnalyzer.Services.Graphics;

namespace LasAnalyzer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private LasFileReader _lasFileReader;
        private DocxWriter _docxWriter;

        private GraphService _graphServiceGamma;
        private GraphService _graphServiceNeutronic;

        private int windowSize;
        private int smoothingIterations;
        private bool isHeatingSelected;
        private bool isCoolingSelected;
        private bool isGammaSelected;
        private bool isNeutronicSelected;
        private ZoomAndPanMode _zoomMode;

        public GraphData LasDataForGamma { get; set; }
        public GraphData LasDataForNeutronic { get; set; }

        public ZoomAndPanMode ZoomMode
        {
            get => _zoomMode;
            set => this.RaiseAndSetIfChanged(ref _zoomMode, value);
        }

        public GraphService GraphServiceGamma
        {
            get => _graphServiceGamma;
            set => this.RaiseAndSetIfChanged(ref _graphServiceGamma, value);
        }

        public GraphService GraphServiceNeutronic
        {
            get => _graphServiceNeutronic;
            set => this.RaiseAndSetIfChanged(ref _graphServiceNeutronic, value);
        }

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

        public ReactiveCommand<Unit, Unit> OpenLasFileCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenGraphWindowCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateAndSaveReportCommand { get; }

        public ReactiveCommand<PointerCommandArgs, Unit> PointerDownCommand { get; }
        public ReactiveCommand<PointerCommandArgs, Unit> PointerMoveCommand { get; }
        public ReactiveCommand<PointerCommandArgs, Unit> PointerUpCommand { get; }

        public ReactiveCommand<Unit, Unit> EnableMovementChartComand { get; }
        public ReactiveCommand<Unit, Unit> EnableMovementVerticalLinesComand { get; }
        public ReactiveCommand<Unit, Unit> EnableMovementPointsComand { get; }

        public MainWindowViewModel()
        {
            _lasFileReader = new LasFileReader();
            _docxWriter = new DocxWriter();

            OpenLasFileCommand = ReactiveCommand.CreateFromTask(OpenLasFileAsync);
            OpenGraphWindowCommand = ReactiveCommand.Create(OpenGraphWindow);
            CreateAndSaveReportCommand = ReactiveCommand.Create(CreateAndSaveReport);

            MessageBus.Current.Listen<GraphData>("GraphDataMessage")
            .Subscribe(graphData => ReceiveGraphData(graphData)); ///

            GraphServiceGamma = new GraphService(("RSD", "RLD"));
            GraphServiceNeutronic = new GraphService(("NTNC", "FTNC"));

            PointerDownCommand = ReactiveCommand.Create<PointerCommandArgs>(PointerDown);
            PointerMoveCommand = ReactiveCommand.Create<PointerCommandArgs>(PointerMove);
            PointerUpCommand = ReactiveCommand.Create<PointerCommandArgs>(PointerUp);

            EnableMovementChartComand = ReactiveCommand.Create(EnableMovementChart);
            EnableMovementVerticalLinesComand = ReactiveCommand.Create(EnableMovementVerticalLines);
            EnableMovementPointsComand = ReactiveCommand.Create(EnableMovementPoints);

            YAxis = new[] 
            { new Axis()
            };

            ZoomMode = ZoomAndPanMode.X;
            WindowSize = 60;
            SmoothingIterations = 3;
            IsHeatingSelected = true;
            IsCoolingSelected = true;
            IsGammaSelected = true;
        }
        private Axis[] invisibleY;

        public Axis[] YAxis
        {
            get => invisibleY;
            set => this.RaiseAndSetIfChanged(ref invisibleY, value);
        }

        private void EnableMovementChart()
        {
            ZoomMode = ZoomMode == ZoomAndPanMode.ZoomX ? ZoomAndPanMode.X : ZoomAndPanMode.ZoomX;
            GraphServiceGamma.IsEnabledMovementVertLines = false;
            GraphServiceGamma.IsEnabledMovementPoints = false;

            GraphServiceNeutronic.IsEnabledMovementVertLines = false;
            GraphServiceNeutronic.IsEnabledMovementPoints = false;
        }

        private void EnableMovementVerticalLines()
        {
            ZoomMode = ZoomAndPanMode.ZoomX;
            GraphServiceGamma.IsEnabledMovementVertLines = !GraphServiceGamma.IsEnabledMovementVertLines;
            GraphServiceGamma.IsEnabledMovementPoints = false;

            GraphServiceNeutronic.IsEnabledMovementVertLines = !GraphServiceNeutronic.IsEnabledMovementVertLines;
            GraphServiceNeutronic.IsEnabledMovementPoints = false;
        }

        private void EnableMovementPoints()
        {
            ZoomMode = ZoomAndPanMode.ZoomX;
            GraphServiceGamma.IsEnabledMovementVertLines = false;
            GraphServiceGamma.IsEnabledMovementPoints = !GraphServiceGamma.IsEnabledMovementPoints;

            GraphServiceNeutronic.IsEnabledMovementVertLines = false;
            GraphServiceNeutronic.IsEnabledMovementPoints = !GraphServiceNeutronic.IsEnabledMovementPoints;
        }

        private bool isEnableMaxPointMarking = false;
        private bool isEnableMinPointMarking = false;

        private bool isDragging = false;
        private LvcPointD lastPointerPosition;

        private void PointerDown(PointerCommandArgs args)
        {
            if (isEnableMaxPointMarking)
            {
                var chart = (ICartesianChartView<SkiaSharpDrawingContext>)args.Chart;
                var lastPointerPosition = chart.ScalePixelsToData(args.PointerPosition);

                //chart.Series.ToArray()[1].Values.
                //GraphServiceGamma
            }
            else if (isEnableMinPointMarking)
            {
                var chart = (ICartesianChartView<SkiaSharpDrawingContext>)args.Chart;
                var lastPointerPosition = chart.ScalePixelsToData(args.PointerPosition);
            }
            else
            {
                isDragging = true;
                var chart = (ICartesianChartView<SkiaSharpDrawingContext>)args.Chart;
                lastPointerPosition = chart.ScalePixelsToData(args.PointerPosition);
                var thumb = GraphServiceGamma.Thumbs[0];

                // update the scroll bar thumb when the user is dragging the chart
                thumb.Xi = lastPointerPosition.X;
                thumb.Xj = lastPointerPosition.X;
            }
        }

        private void PointerMove(PointerCommandArgs args)
        {
            if (!isDragging) return;

            var chart = (ICartesianChartView<SkiaSharpDrawingContext>)args.Chart;
            lastPointerPosition = chart.ScalePixelsToData(args.PointerPosition);

            var thumb = GraphServiceGamma.Thumbs[0];

            // update the scroll bar thumb when the user is dragging the chart
            thumb.Xi = lastPointerPosition.X;
            thumb.Xj = lastPointerPosition.X;
        }

        private void PointerUp(PointerCommandArgs args)
        {
            isDragging = false;
        }

        private void ReceiveGraphData(GraphData graphData)
        {
            LasDataForGamma = graphData; /// for graph window
        }
        
        private void CreateAndSaveReport()
        {
            _docxWriter.CreateAndSaveReport(
                GraphServiceGamma,
                GraphServiceNeutronic,
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

                GraphServiceGamma = new GraphService(lasData.Item1, ("RSD", "RLD"), WindowSize);
                GraphServiceNeutronic = new GraphService(lasData.Item2, ("NTNC", "FTNC"), WindowSize);

                YAxis = new[]
                {
                    new Axis
                    {
                        //MaxLimit = LasDataForGamma.NearProbe.Max() * 1.1,
                        //MinLimit = LasDataForGamma.NearProbe.Min() * 0.9
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