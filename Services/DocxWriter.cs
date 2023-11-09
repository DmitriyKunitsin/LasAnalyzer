using Avalonia.Controls;
using LasAnalyzer.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Document.NET;
using Xceed.Words.NET;
using Avalonia.Media.Imaging;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LasAnalyzer.Services.Graphics;
using DynamicData;

namespace LasAnalyzer.Services
{
    public class DocxWriter
    {
        private void CreateReport(ReportModel report, string outputPath)
        {
            using (DocX document = DocX.Create(outputPath))
            {
                document.InsertParagraph("Протокол").Bold().FontSize(16).Alignment = Alignment.center;
                document.InsertParagraph("температурных испытаний прибора").FontSize(12).Alignment = Alignment.center;
                document.InsertParagraph();

                document.InsertParagraph("1. Прибор №: " + report.SerialNumber).FontSize(12);
                document.InsertParagraph("2. Канал: " + report.DeviceType).FontSize(12);
                document.InsertParagraph("3. Дата: " + report.TestDate).FontSize(12);
                document.InsertParagraph("4. Пороги: RSD – " + report.NearProbeThreshold + " мВ, RLD – " + report.FarProbeThreshold + " мВ").FontSize(12);
                document.InsertParagraph();

                document.InsertParagraph(report.NearProbeTitle).FontSize(12);
                InsertChartImageToDocX(document, report.Graphs[0]);
                document.InsertParagraph(report.FarProbeTitle).FontSize(12);
                InsertChartImageToDocX(document, report.Graphs[1]);
                document.InsertParagraph($"{report.FarProbeTitle}/{report.NearProbeTitle}").FontSize(12);
                InsertChartImageToDocX(document, report.Graphs[2]);
                document.InsertParagraph("TEMPER").FontSize(12);
                InsertChartImageToDocX(document, report.Graphs[3]);

                document.InsertParagraph();

                InsertResultTables(document, report);
                document.InsertParagraph();

                document.InsertParagraph("10. Выводы");
                document.InsertParagraph(report.Conclusion);

                document.Save();
            }
        }

        private void InsertChartImageToDocX(DocX document, byte[] imageBytes)
        {
            var image = document.AddImage(new MemoryStream(imageBytes));
            Picture picture = image.CreatePicture();
            // Задайте размер и позицию изображения по вашим требованиям
            picture.Width = 500;
            picture.Height = 200;
            document.InsertParagraph().AppendPicture(picture);
        }

        private void InsertResultTables(DocX document, ReportModel report)
        {
            foreach (var resultTable in report.Results)
            {
                var resultString = resultTable.TempType == TempType.Heating ? "при нагреве" : "при охлаждении";
                document.InsertParagraph($"9. Результаты {resultString}").FontSize(12); ;

                var table = document.AddTable(resultTable.Results.Count + 1, 5);
                table.Design = TableDesign.TableGrid;
                table.Alignment = Alignment.center;

                table = setFormulasAndHeaders(table, resultTable.TemperBase, report);

                for (int i = 0; i < resultTable.Results.Count; i++)
                {
                    //table.Rows[i].Cells[0].Paragraphs.First().InsertText(resultTable.Results[i].Num.ToString());
                    //table.Rows[i].Cells[1].Paragraphs.First().InsertText(resultTable.Results[i].Formula);
                    table.Rows[i + 1].Cells[2].Paragraphs.First().InsertText(resultTable.Results[i].NearProbe.ToString());
                    table.Rows[i + 1].Cells[3].Paragraphs.First().InsertText(resultTable.Results[i].FarProbe.ToString());
                    table.Rows[i + 1].Cells[4].Paragraphs.First().InsertText(resultTable.Results[i].FarToNearProbeRatio.ToString());
                }
                document.InsertTable(table);
            }
        }

        private Table setFormulasAndHeaders(Table table, double? baseTemper, ReportModel report)
        {
            // suda lu4we ne smotret
            table.Rows[0].Cells[0].Paragraphs.First().InsertText($"п/п");
            table.Rows[0].Cells[1].Paragraphs.First().InsertText($"Формула");
            table.Rows[0].Cells[2].Paragraphs.First().InsertText(report.NearProbeTitle);
            table.Rows[0].Cells[3].Paragraphs.First().InsertText(report.FarProbeTitle);
            table.Rows[0].Cells[4].Paragraphs.First().InsertText($"{report.FarProbeTitle}/{report.NearProbeTitle}");

            table.Rows[1].Cells[1].Paragraphs.First().InsertText($"N(T={baseTemper.Value})");
            table.Rows[2].Cells[1].Paragraphs.First().InsertText("MAX/T");
            table.Rows[3].Cells[1].Paragraphs.First().InsertText("MIN/T");
            table.Rows[4].Cells[1].Paragraphs.First().InsertText($"MAX - N(T={baseTemper.Value})");
            table.Rows[5].Cells[1].Paragraphs.First().InsertText($"N(T={baseTemper.Value}) - MIN");
            table.Rows[6].Cells[1].Paragraphs.First().InsertText("% MAX");
            table.Rows[7].Cells[1].Paragraphs.First().InsertText("% MIN");

            for (int i = 1; i < 7; i++)
            {
                table.Rows[i].Cells[0].Paragraphs.First().InsertText((i).ToString());
            }

            return table;
        }

        public void CreateAndSaveReport(
            GraphService graphServiceGamma,
            GraphService graphServiceNeutronic,
            bool isHeatingSelected,
            bool isCoolingSelected
        )
        {
            // todo: another condition for save report, mb use property IsDataSetted in GraphService
            if (graphServiceGamma.GraphNearProbe.Data is not null)
            {
                // todo: create and fill reportModel in MainWindowViewModel
                SaveReport(
                    graphServiceGamma,
                    DeviceType.Gamma,
                    isHeatingSelected,
                    isCoolingSelected,
                    "RSD",
                    "RLD"
                );
            }
            if (graphServiceNeutronic.GraphNearProbe.Data is not null)
            {
                SaveReport(
                    graphServiceNeutronic,
                    DeviceType.Neutronic,
                    isHeatingSelected,
                    isCoolingSelected,
                    "NTNC",
                    "FTNC"
                );
            }
        }

        private void SaveReport(
            GraphService graphService,
            DeviceType deviceType,
            bool isHeatingSelected,
            bool isCoolingSelected,
            string nearProbeTitle,
            string farProbeTitle
        )
        {
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
            foreach (var item in results)
            {
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
            var max = graphService.GraphTemperature.Data.Max();
            var heatRange = isHeating ? $"от {minLeft} до {max} градусов" : "";
            var coolRange = isCooling ? $"от {max} до {minRight} градусов" : "";
            var Kek = isHeating && isCooling ? "и " : "";

            var tempRange = $"{heatRange} {Kek}{coolRange}";

            var conditionForConclusion = results[0].ThresholdExceeded || results[1].ThresholdExceeded ? "превышает" : "не превышает";
            var departureThreshold = deviceType == DeviceType.Gamma ? 5 : 6;

            ReportModel ReportModel = new ReportModel()
            {
                SerialNumber = "12312312",
                DeviceType = "gg nn",
                TestDate = "11.22.33",
                NearProbeThreshold = 0,
                FarProbeThreshold = 0,
                NearProbeTitle = nearProbeTitle,
                FarProbeTitle = farProbeTitle,
                Graphs = chartImageDatas,
                Results = results,
                Conclusion = $"Температурный уход сигналов {nearProbeTitle}, {farProbeTitle} и {farProbeTitle}/{nearProbeTitle} в диапазоне температур {tempRange} {conditionForConclusion} {departureThreshold}%."
            };
            CreateReport(ReportModel, Directory.GetCurrentDirectory() + $"\\{ReportModel.SerialNumber}_{ReportModel.DeviceType}_{ReportModel.TestDate}.docx");
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
                Height = 400,
                Series = new ISeries[]
                {
                    new LineSeries<double?> { Values = data },
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

        private void CreateChart(DocX document, List<double> graphData)
        {
            // Create a line chart.
            var lineChart = document.AddChart<LineChart>();
            lineChart.AddLegend(ChartLegendPosition.Right, false);

            // Create and add series by binding X and Y.
            var series = new Series("RSD");
            series.Bind(graphData, "X-Axis", "Y-Axis");
            lineChart.AddSeries(series);

            // Insert chart into document
            document.InsertChart(lineChart);
            
        }
    }
}
