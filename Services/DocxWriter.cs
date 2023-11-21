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
        public void CreateReport(ReportModel report, string outputPath)
        {
            using (DocX document = DocX.Create(outputPath))
            {
                document.InsertParagraph("Протокол").Bold().FontSize(16).Alignment = Alignment.center;
                document.InsertParagraph("температурных испытаний прибора").FontSize(12).Alignment = Alignment.center;
                document.InsertParagraph();

                document.InsertParagraph("1. Прибор: \t\t№: " + report.SerialNumber).FontSize(12);
                document.InsertParagraph("2. Канал: \t\t" + report.DeviceType).FontSize(12);
                document.InsertParagraph("3. Дата испытаний: \t" + report.TestDate).FontSize(12);
                document.InsertParagraph("4. Пороги: \t\tRSD – " + report.NearProbeThreshold + " мВ, RLD – " + report.FarProbeThreshold + " мВ").FontSize(12);

                document.InsertParagraph(report.NearProbeTitle).FontSize(12);
                InsertChartImageToDocX(document, report.Graphs[0]);
                document.InsertParagraph(report.FarProbeTitle).FontSize(12);
                InsertChartImageToDocX(document, report.Graphs[1]);
                document.InsertParagraph($"{report.FarProbeTitle}/{report.NearProbeTitle}").FontSize(12);
                InsertChartImageToDocX(document, report.Graphs[2]);
                document.InsertParagraph();
                document.InsertParagraph("TEMPER").FontSize(12);
                InsertChartImageToDocX(document, report.Graphs[3]);

                document.InsertParagraph();

                InsertResultTables(document, report);
                document.InsertParagraph();

                document.InsertParagraph("10. Выводы").FontSize(12);
                document.InsertParagraph(report.Conclusion).FontSize(12);
                document.InsertParagraph();
                document.InsertParagraph("Термоиспытания провел:").FontSize(12);

                document.Save();
            }
        }

        private void InsertChartImageToDocX(DocX document, byte[] imageBytes)
        {
            var image = document.AddImage(new MemoryStream(imageBytes));
            Picture picture = image.CreatePicture();
            picture.Width = 500;
            picture.Height = 175;
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

                table = SetFormulasAndHeaders(table, resultTable.TemperBase, report);

                for (int i = 0; i < resultTable.Results.Count; i++)
                {
                    if (i == 1 || i == 2)
                    {
                        table.Rows[i + 1].Cells[2].Paragraphs.First()
                            .InsertText($"{resultTable.Results[i].NearProbe} ({resultTable.Results[i].Temperatures.NearProbe})");
                        table.Rows[i + 1].Cells[3].Paragraphs.First()
                            .InsertText($"{resultTable.Results[i].FarProbe} ({resultTable.Results[i].Temperatures.FarProbe})");
                        table.Rows[i + 1].Cells[4].Paragraphs.First()
                            .InsertText($"{resultTable.Results[i].FarToNearProbeRatio} ({resultTable.Results[i].Temperatures.FarToNearProbeRatio})");
                    }
                    else
                    {
                        table.Rows[i + 1].Cells[2].Paragraphs.First().InsertText(resultTable.Results[i].NearProbe.ToString());
                        table.Rows[i + 1].Cells[3].Paragraphs.First().InsertText(resultTable.Results[i].FarProbe.ToString());
                        table.Rows[i + 1].Cells[4].Paragraphs.First().InsertText(resultTable.Results[i].FarToNearProbeRatio.ToString());
                    }

                    if (i >= 5)
                    {
                        table.Rows[i + 1].Cells[2].Paragraphs.First().Bold();
                        table.Rows[i + 1].Cells[3].Paragraphs.First().Bold();
                        table.Rows[i + 1].Cells[4].Paragraphs.First().Bold();
                    }
                }
                document.InsertTable(table);
            }
        }

        private Table SetFormulasAndHeaders(Table table, double? baseTemper, ReportModel report)
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

            for (int i = 0; i < 7; i++)
            {
                table.Rows[i + 1].Cells[0].Paragraphs.First().InsertText((i + 1).ToString());
            }

            return table;
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
