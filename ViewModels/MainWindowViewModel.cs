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
using System.Web;
using Looch.LasParser;

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

        public LasParser LasData { get; set; }
        //public GraphData LasDataForGamma { get; set; }
        //public GraphData LasDataForNeutronic { get; set; }

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

        public ReactiveCommand<Unit, Unit> OpenLasFileCommand { get; }
        public ReactiveCommand<Unit, Unit> RebuildGraphsCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenGraphWindowCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenResultTableWindowCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateAndSaveReportCommand { get; }

        public ReactiveCommand<Unit, Unit> EnableMovementChartCommand { get; }
        public ReactiveCommand<Unit, Unit> EnableMovementVerticalLinesCommand { get; }
        public ReactiveCommand<Unit, Unit> EnableMovementPointsCommand { get; }

        //public ReactiveCommand<Unit, Unit> CropGammaDataCommand { get; }
        //public ReactiveCommand<Unit, Unit> CropNeutronicDataCommand { get; }

        public MainWindowViewModel()
        {
            _lasFileReader = new LasFileReader();
            _docxWriter = new DocxWriter();

            OpenLasFileCommand = ReactiveCommand.CreateFromTask(GetLasData);
            RebuildGraphsCommand = ReactiveCommand.Create(RebuildGraphs);
            OpenGraphWindowCommand = ReactiveCommand.Create(OpenGraphWindow);
            OpenResultTableWindowCommand = ReactiveCommand.Create(OpenResultTableWindow);
            CreateAndSaveReportCommand = ReactiveCommand.Create(CreateAndSaveReport);

            //MessageBus.Current.Listen<GraphData>("GraphDataMessage")
            //.Subscribe(graphData => ReceiveGraphData(graphData)); ///

            GraphServiceGamma = new GraphService(("RSD", "RLD"));
            GraphServiceNeutronic = new GraphService(("NTNC", "FTNC"));

            EnableMovementChartCommand = ReactiveCommand.Create(EnableMovementChart);
            EnableMovementVerticalLinesCommand = ReactiveCommand.Create(EnableMovementVerticalLines);
            EnableMovementPointsCommand = ReactiveCommand.Create(EnableMovementPoints);

            //CropGammaDataCommand = ReactiveCommand.Create(GraphServiceGamma.CropData);
            //CropNeutronicDataCommand = ReactiveCommand.Create(GraphServiceNeutronic.CropData);

            ZoomMode = ZoomAndPanMode.X;
            WindowSize = 60;
            SmoothingIterations = 3;
            IsHeatingSelected = true;
            IsCoolingSelected = true;
            IsGammaSelected = true;
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

        private void OpenResultTableWindow()
        {
            var calculationTable = new CalculationTable();
            //MessageBus.Current.SendMessage(LasDataForGamma, "GraphDataMessage");
            calculationTable.Show();
        }

        //private void ReceiveGraphData(GraphData graphData)
        //{
        //    LasDataForGamma = graphData; /// for graph window
        //}

        private void RebuildGraphs()
        {
            SetGammaData();
            SetNeutronicData();
        }

        private void CreateAndSaveReport()
        {
            ReportWrapper ReportWrapper = new ReportWrapper();
            var Reports = ReportWrapper.PrepareReport(
                LasData,
                GraphServiceGamma,
                GraphServiceNeutronic,
                IsHeatingSelected,
                IsCoolingSelected
            );

            if (!Directory.Exists($"{Directory.GetCurrentDirectory()}\\output"))
                Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}\\output");

            foreach (var report in Reports)
            {
                _docxWriter.CreateReport(report, $"{Directory.GetCurrentDirectory()}\\output\\{report.SerialNumber}_{report.DeviceType}_{report.TestDate}.docx");
            }
        }

        private void OpenGraphWindow()
        {
            var graphWindow = ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Windows.OfType<GraphWindow>().FirstOrDefault();

            if (graphWindow == null)
            {
                graphWindow = new GraphWindow();
                //MessageBus.Current.SendMessage(LasDataForGamma, "GraphDataMessage");
                graphWindow.Show();
            }
            else
            {
                graphWindow.Activate();
            }
        }

        private async Task GetLasData()
        {
            var lasData = await _lasFileReader.GetLasData();
            if (lasData is null)
                return;

            LasData = lasData;

            // todo: сделать 1 функцию которая возвращает GraphService
            //if (HasGamma(LasData))
            SetGammaData();
            //if (HasNeutonic(LasData))
            SetNeutronicData();
        }

        private void SetGammaData()
        {
            if (!LasData.Data.ContainsKey("RSD"))
            {
                GraphServiceGamma = new GraphService(("RSD", "RLD"));
                return;
            }

            // todo: temper for realdepth (not MT)
            // todo: fit temper length to smoothed data length
            var smoothedNearProbeData = DataProcessor.SmoothDataWithCount(LasData.Data["RSD"].ToList(), WindowSize, SmoothingIterations);
            var smoothedFarProbeData = DataProcessor.SmoothDataWithCount(LasData.Data["RLD"].ToList(), WindowSize, SmoothingIterations);
            var smoothedFarToNearRatio = DataProcessor.DivideArrays(smoothedFarProbeData, smoothedNearProbeData);

            var tempKey = "MT";
            if (!LasData.Data.ContainsKey("MT") && LasData.Data.ContainsKey("T_GGKP"))
            {
                tempKey = "T_GGKP";
            }

            var graphData = new GraphData
            {
                NearProbe = smoothedNearProbeData,
                FarProbe = smoothedFarProbeData,
                FarToNearProbeRatio = smoothedFarToNearRatio,
                Temperature = LasData.Data[tempKey].Skip(LasData.Data[tempKey].Length - smoothedNearProbeData.Count).ToList(),
                Time = LasData.Data["TIME"].Skip(LasData.Data[tempKey].Length - smoothedNearProbeData.Count).ToList()
            };

            GraphServiceGamma = new GraphService(graphData, ("RSD", "RLD"), WindowSize);
        }

        private void SetNeutronicData()
        {
            if (!LasData.Data.ContainsKey("NTNC"))
            {
                GraphServiceNeutronic = new GraphService(("NTNC", "FTNC"));
                return;
            }

            var smoothedNearProbeData = DataProcessor.SmoothDataWithCount(LasData.Data["NTNC"].ToList(), WindowSize, SmoothingIterations);
            var smoothedFarProbeData = DataProcessor.SmoothDataWithCount(LasData.Data["FTNC"].ToList(), WindowSize, SmoothingIterations);
            var smoothedFarToNearRatio = DataProcessor.DivideArrays(smoothedFarProbeData, smoothedNearProbeData);

            var tempKey = "MT";
            if (!LasData.Data.ContainsKey("MT") && LasData.Data.ContainsKey("T_GGKP"))
            {
                tempKey = "T_GGKP";
            }

            var graphData = new GraphData
            {
                NearProbe = smoothedNearProbeData,
                FarProbe = smoothedFarProbeData,
                FarToNearProbeRatio = smoothedFarToNearRatio,
                Temperature = LasData.Data[tempKey].Skip(LasData.Data[tempKey].Length - smoothedNearProbeData.Count).ToList(),
                Time = LasData.Data["TIME"].Skip(LasData.Data[tempKey].Length - smoothedNearProbeData.Count).ToList()
            };

            GraphServiceNeutronic = new GraphService(graphData, ("NTNC", "FTNC"), WindowSize);
        }
    }
}