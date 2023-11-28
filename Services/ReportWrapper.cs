using LasAnalyzer.Models;
using LasAnalyzer.Services.Graphics;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveChartsCore.SkiaSharpView.VisualElements;
using Looch.LasParser;

namespace LasAnalyzer.Services
{
    public class ReportWrapper
    {
        public List<ReportModel> CreateAndSaveReports(
            LasParser LasData,
            GraphService graphServiceGamma,
            GraphService graphServiceNeutronic,
            bool isHeatingSelected,
            bool isCoolingSelected
        )
        {
            var reports = new List<ReportModel>();

            var serialNumberIdx = LasData.Wmnem.IndexOf("SNUM");
            var serialNumber = serialNumberIdx != -1 ? LasData.Wvalue[serialNumberIdx] : string.Empty;
            var dateIdx = LasData.Wmnem.IndexOf("DATE");
            var date = dateIdx != -1 ? LasData.Wvalue[dateIdx].Replace("/", ".") : string.Empty;

            // todo: another condition for save report, mb use property IsDataSetted in GraphService
            // todo: choice which data to save, gamma or neutronic
            if (graphServiceGamma.GraphNearProbe.Data is not null)
            {
                var nearProbeThreshold = LasData.Data.ContainsKey("THLDS") ? LasData.Data["THLDS"][0].ToString() : "\t\t";
                var farProbeThreshold = LasData.Data.ContainsKey("THLDL") ? LasData.Data["THLDL"][0].ToString() : "\t\t";
                reports.Add(SaveReport(
                    graphServiceGamma,
                    DeviceType.Gamma,
                    isHeatingSelected,
                    isCoolingSelected,
                    "RSD",
                    "RLD",
                    serialNumber,
                    date,
                    nearProbeThreshold,
                    farProbeThreshold
                ));
            }
            if (graphServiceNeutronic.GraphNearProbe.Data is not null)
            {
                reports.Add(SaveReport(
                    graphServiceNeutronic,
                    DeviceType.Neutronic,
                    isHeatingSelected,
                    isCoolingSelected,
                    "NTNC",
                    "FTNC",
                    serialNumber,
                    date,
                    nearProbeThreshold: "\t\t",
                    farProbeThreshold: "\t\t"
                ));
            }
            return reports;
        }

        private ReportModel SaveReport(
            GraphService graphService,
            DeviceType deviceType,
            bool isHeatingSelected,
            bool isCoolingSelected,
            string nearProbeTitle,
            string farProbeTitle,
            string serialNumber,
            string testDate,
            string nearProbeThreshold,
            string farProbeThreshold
        )
        {
            // todo: need refactoring of this func
            List<byte[]> chartImageDatas = new List<byte[]>()
                {
                    createChartImage(graphService.GraphNearProbe.Data, graphService.GraphNearProbe.Title),
                    createChartImage(graphService.GraphFarProbe.Data, graphService.GraphFarProbe.Title),
                    createChartImage(graphService.GraphFarToNearProbeRatio.Data, graphService.GraphFarToNearProbeRatio.Title),
                    createChartImage(graphService.GraphTemperature.Data, graphService.GraphTemperature.Title)
                };

            var results = CalculatorWrapper(
                graphService,
                deviceType,
                isHeatingSelected,
                isCoolingSelected
            );

            bool isHeating = false;
            bool isCooling = false;
            int minLeft = 0;
            int minRight = 0;
            var thresholdExceeded = false;
            foreach (var item in results)
            {
                thresholdExceeded = thresholdExceeded || item.ThresholdExceeded;
                if (item.TempType == TempType.Heating)
                {
                    isHeating = true;
                    minLeft = Convert.ToInt32(item.TemperBase);
                }
                if (item.TempType == TempType.Cooling)
                {
                    isCooling = true;
                    minRight = Convert.ToInt32(item.TemperBase);
                }
            }
            var thresholdExceededStr = thresholdExceeded ? "превышает" : "не превышает";
            var max = graphService.GraphTemperature.Data.Max();
            var heatRange = isHeating ? $"от {minLeft} до {max} градусов" : "";
            var coolRange = isCooling ? $"от {max} до {minRight} градусов" : "";
            var Kek = isHeating && isCooling ? "и " : "";
            var tempRange = $"{heatRange} {Kek}{coolRange}";
            var departureThreshold = deviceType == DeviceType.Gamma ? 5 : 6;

            ReportModel reportModel = new ReportModel
            {
                SerialNumber = serialNumber,
                DeviceType = deviceType == DeviceType.Gamma ? "ГГКП" : "ННКТ",
                TestDate = testDate,
                NearProbeThreshold = nearProbeThreshold,
                FarProbeThreshold = farProbeThreshold,
                NearProbeTitle = nearProbeTitle,
                FarProbeTitle = farProbeTitle,
                Graphs = chartImageDatas,
                Results = results,
                Conclusion = $"Температурный уход сигналов {nearProbeTitle}, {farProbeTitle} и {farProbeTitle}/{nearProbeTitle} в диапазоне температур {tempRange} {thresholdExceededStr} {departureThreshold}%."
            };

            return reportModel;
        }

        private List<ResultTable> CalculatorWrapper(
            GraphService graphService,
            DeviceType deviceType,
            bool isHeatingSelected,
            bool isCoolingSelected
        )
        {
            var calculator = new Calculator();
            var tableList = new List<ResultTable>();

            if ((graphService.TemperatureType == TempType.Heating || graphService.TemperatureType == TempType.Both) && isHeatingSelected)
                tableList.Add(calculator.CalculateMetrics(graphService, deviceType, TempType.Heating));

            if ((graphService.TemperatureType == TempType.Cooling || graphService.TemperatureType == TempType.Both) && isCoolingSelected)
                tableList.Add(calculator.CalculateMetrics(graphService, deviceType, TempType.Cooling));

            return tableList;
        }

        private byte[] createChartImage(List<double?> data, string title)
        {
            var cartesianChart = new SKCartesianChart
            {
                Width = 1000,
                Height = 350,
                Series = new ISeries[]
                {
                    new LineSeries<double?>
                    {
                        Values = data,
                        GeometryStroke = null,
                        GeometryFill = null,
                        Fill = null,
                        Stroke = new SolidColorPaint
                        {
                            Color = SKColors.RoyalBlue,
                            StrokeThickness = 3,
                            ZIndex = 1
                        },
                        LineSmoothness = 0,
                        ZIndex = 1,
                    }
                },
                Title = new LabelVisual
                {
                    Text = title,
                    TextSize = 30,
                    Padding = new Padding(15),
                    Paint = new SolidColorPaint(0xff303030)
                },
                YAxes = new[]
                {
                    new Axis
                    {
                        //MaxLimit = LasDataForGamma.NearProbe.Max() * 1.1,
                        //MinLimit = LasDataForGamma.NearProbe.Min() * 0.9
                    }
                },
                LegendPosition = LiveChartsCore.Measure.LegendPosition.Right,
                Background = SKColors.White
            };

            return cartesianChart.GetImage().Encode().ToArray();
        }
    }
}
