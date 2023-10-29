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

namespace LasAnalyzer.Services
{
    public class DocxWriter
    {
        public void CreateReport(ReportModel report, string outputPath)
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

        private void InsertResultTables(DocX document, List<List<Result>> Results)
        {
            // todo: create model for 1 result table, there are property tempType is heating or is cooling
            foreach (var resultTable in Results)
            {
                document.InsertParagraph("9. Результаты");
                var table = document.AddTable(resultTable.Count, 5);
                table.Design = TableDesign.TableGrid;
                table.Alignment = Alignment.center;
                for (int i = 0; i < resultTable.Count; i++)
                {
                    table.Rows[i].Cells[0].Paragraphs.First().InsertText(resultTable[i].Num.ToString());
                    table.Rows[i].Cells[1].Paragraphs.First().InsertText(resultTable[i].Formula);
                    table.Rows[i].Cells[2].Paragraphs.First().InsertText(resultTable[i].NearProbe.ToString());
                    table.Rows[i].Cells[3].Paragraphs.First().InsertText(resultTable[i].FarProbe.ToString());
                    table.Rows[i].Cells[4].Paragraphs.First().InsertText(resultTable[i].FarToNearProbeRatio.ToString());
                }
                document.InsertTable(table);
            }
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
