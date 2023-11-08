﻿using LasAnalyzer.Services;
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

        private Axis[] invisibleY;
        public Axis[] YAxis
        {
            get => invisibleY;
            set => this.RaiseAndSetIfChanged(ref invisibleY, value);
        }

        public ReactiveCommand<Unit, Unit> OpenLasFileCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenGraphWindowCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateAndSaveReportCommand { get; }

        public ReactiveCommand<Unit, Unit> EnableMovementChartComand { get; }
        public ReactiveCommand<Unit, Unit> EnableMovementVerticalLinesComand { get; }
        public ReactiveCommand<Unit, Unit> EnableMovementPointsComand { get; }

        //public ReactiveCommand<Unit, Unit> CropGammaDataCommand { get; }
        //public ReactiveCommand<Unit, Unit> CropNeutronicDataCommand { get; }

        public MainWindowViewModel()
        {
            _lasFileReader = new LasFileReader();
            _docxWriter = new DocxWriter();

            OpenLasFileCommand = ReactiveCommand.CreateFromTask(GetLasData);
            OpenGraphWindowCommand = ReactiveCommand.Create(OpenGraphWindow);
            CreateAndSaveReportCommand = ReactiveCommand.Create(CreateAndSaveReport);

            //MessageBus.Current.Listen<GraphData>("GraphDataMessage")
            //.Subscribe(graphData => ReceiveGraphData(graphData)); ///

            GraphServiceGamma = new GraphService(("RSD", "RLD"));
            GraphServiceNeutronic = new GraphService(("NTNC", "FTNC"));

            EnableMovementChartComand = ReactiveCommand.Create(EnableMovementChart);
            EnableMovementVerticalLinesComand = ReactiveCommand.Create(EnableMovementVerticalLines);
            EnableMovementPointsComand = ReactiveCommand.Create(EnableMovementPoints);

            //CropGammaDataCommand = ReactiveCommand.Create(GraphServiceGamma.CropData);
            //CropNeutronicDataCommand = ReactiveCommand.Create(GraphServiceNeutronic.CropData);

            YAxis = new[]
                {
                    new Axis
                    {
                        //MaxLimit = LasDataForGamma.NearProbe.Max() * 1.1,
                        //MinLimit = LasDataForGamma.NearProbe.Min() * 0.9
                    }
                };

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

        //private void ReceiveGraphData(GraphData graphData)
        //{
        //    LasDataForGamma = graphData; /// for graph window
        //}
        
        private void CreateAndSaveReport()
        {
            _docxWriter.CreateAndSaveReport(
                GraphServiceGamma,
                GraphServiceNeutronic,
                IsHeatingSelected,
                IsCoolingSelected
            );
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

            SetGammaData();
            SetNeutronicData();
        }

        private void SetGammaData()
        {
            if (!LasData.Data.ContainsKey("RSD"))
            {
                GraphServiceGamma = new GraphService(("RSD", "RLD"));
                return;
            }

            // todo: temper for realdepth
            // todo: fit temper length to smoothed data length
            var smoothedNearProbeData = DataProcessor.SmoothDataWithCount(LasData.Data["RSD"].ToList(), WindowSize, SmoothingIterations);
            var smoothedFarProbeData = DataProcessor.SmoothDataWithCount(LasData.Data["RLD"].ToList(), WindowSize, SmoothingIterations);
            var smoothedFarToNearRatio = DataProcessor.DivideArrays(smoothedFarProbeData, smoothedNearProbeData);

            var graphData = new GraphData
            {
                NearProbe = smoothedNearProbeData,
                FarProbe = smoothedFarProbeData,
                FarToNearProbeRatio = smoothedFarToNearRatio,
                Temperature = LasData.Data["MT"].ToList(),
                Time = LasData.Data["TIME"].ToList()
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

            var graphData = new GraphData
            {
                NearProbe = smoothedNearProbeData,
                FarProbe = smoothedFarProbeData,
                FarToNearProbeRatio = smoothedFarToNearRatio,
                Temperature = LasData.Data["MT"].ToList(),
                Time = LasData.Data["TIME"].ToList()
            };

            GraphServiceNeutronic = new GraphService(graphData, ("NTNC", "FTNC"), WindowSize);
        }
    }
}