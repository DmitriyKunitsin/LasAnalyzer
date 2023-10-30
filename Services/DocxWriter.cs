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

namespace LasAnalyzer.Services
{
    public class DocxWriter
    {
        private void CreateReport(ReportModel report, string outputPath)
        {
            using (DocX document = DocX.Create(outputPath))
            {
                document.InsertParagraph("Протокол температурных испытаний прибора").Bold().FontSize(16).Alignment = Alignment.center;
                document.InsertParagraph();

                document.InsertParagraph("1. Прибор №: " + report.SerialNumber);
                document.InsertParagraph("2. Канал: " + report.DeviceType);
                document.InsertParagraph("3. Дата: " + report.TestDate);
                document.InsertParagraph("4. Пороги: RSD – " + report.NearProbeThreshold + " мВ, RLD – " + report.FarProbeThreshold + " мВ");
                document.InsertParagraph();

                //document.InsertParagraph(graph.Title).Bold();
                //CreateChart(document, report.Graphs.NearProbe);
                //CreateChart(document, report.Graphs.FarProbe);
                //CreateChart(document, report.Graphs.FarToNearProbeRatio);
                //CreateChart(document, report.Graphs.Temperature);
                for (int i = 0; i < report.Graphs.Count; i++)
                {
                    InsertChartImageToDocX(document, report.Graphs[i]);
                }
                document.InsertParagraph();

                InsertResultTables(document, report.Results);
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

        private void InsertResultTables(DocX document, List<ResultTable> Results)
        {
            foreach (var resultTable in Results)
            {
                document.InsertParagraph("9. Результаты");
                var table = document.AddTable(resultTable.Results.Count, 5);
                table.Design = TableDesign.TableGrid;
                table.Alignment = Alignment.center;

                table = setFormulas(table, resultTable.TemperBase);

                for (int i = 0; i < resultTable.Results.Count; i++)
                {
                    //table.Rows[i].Cells[0].Paragraphs.First().InsertText(resultTable.Results[i].Num.ToString());
                    //table.Rows[i].Cells[1].Paragraphs.First().InsertText(resultTable.Results[i].Formula);
                    table.Rows[i].Cells[2].Paragraphs.First().InsertText(resultTable.Results[i].NearProbe.ToString());
                    table.Rows[i].Cells[3].Paragraphs.First().InsertText(resultTable.Results[i].FarProbe.ToString());
                    table.Rows[i].Cells[4].Paragraphs.First().InsertText(resultTable.Results[i].FarToNearProbeRatio.ToString());
                }
                document.InsertTable(table);
            }
        }

        private Table setFormulas(Table table, double baseTemper)
        {
            // suda lu4we ne smotret
            table.Rows[0].Cells[1].Paragraphs.First().InsertText($"N(T={baseTemper})");
            table.Rows[1].Cells[1].Paragraphs.First().InsertText("MAX/T");
            table.Rows[2].Cells[1].Paragraphs.First().InsertText("MIN/T");
            table.Rows[3].Cells[1].Paragraphs.First().InsertText($"MAX - N(T={baseTemper})");
            table.Rows[4].Cells[1].Paragraphs.First().InsertText($"N(T={baseTemper}) - MIN");
            table.Rows[5].Cells[1].Paragraphs.First().InsertText("% MAX");
            table.Rows[6].Cells[1].Paragraphs.First().InsertText("% MIN");

            for (int i = 0; i < 7; i++)
            {
                table.Rows[i].Cells[0].Paragraphs.First().InsertText((i + 1).ToString());
            }

            return table;
        }

        public void CreateAndSaveReport(
            GraphData LasDataForGamma,
            GraphData LasDataForNeutronic,
            bool isHeatingSelected,
            bool isCoolingSelected,
            int windowSize
        )
        {
            if (LasDataForGamma is not null)
            {
                // todo: create and fill reportModel in MainWindowViewModel
                SaveReport(
                    LasDataForGamma,
                    "RSD",
                    "RLD",
                    isHeatingSelected,
                    isCoolingSelected,
                    windowSize
                );
            }
            if (LasDataForNeutronic is not null)
            {
                SaveReport(
                    LasDataForNeutronic,
                    "NTNC",
                    "FTNC",
                    isHeatingSelected,
                    isCoolingSelected,
                    windowSize
                );
            }
        }

        private void SaveReport(
            GraphData lasData,
            string nearProbeTitle,
            string farProbeTitle,
            bool isHeatingSelected,
            bool isCoolingSelected,
            int windowSize
        )
        {
            List<byte[]> chartImageDatas = new List<byte[]>()
                {
                    createChartImage(lasData.NearProbe, nearProbeTitle),
                    createChartImage(lasData.FarProbe, farProbeTitle),
                    createChartImage(lasData.FarToNearProbeRatio, $"{nearProbeTitle}/{farProbeTitle}"),
                    createChartImage(lasData.Temperature, "TEMPER")
                };

            ReportModel ReportModel = new ReportModel()
            {
                SerialNumber = "12312312",
                DeviceType = "gg nn",
                TestDate = "11.22.33",
                NearProbeThreshold = 0,
                FarProbeThreshold = 0,
                Graphs = chartImageDatas,
                Results = CalculatorWrapper(
                    lasData,
                    isHeatingSelected,
                    isCoolingSelected,
                    windowSize
                ),
                Conclusion = "> < 5 %"
            };
            CreateReport(ReportModel, Directory.GetCurrentDirectory() + "\\out.docx");
        }

        private List<ResultTable> CalculatorWrapper(
            GraphData lasData,
            bool isHeatingSelected,
            bool isCoolingSelected,
            int windowSize
        )
        {
            var calculator = new Calculator();
            var tableList = new List<ResultTable>();

            if (isHeatingSelected)
                tableList.Add(calculator.CalculateMetrics(lasData, TempType.Heating, windowSize));

            if (isCoolingSelected)
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
