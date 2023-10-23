using Avalonia.Controls;
using LasAnalyzer.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Document.NET;
using Xceed.Words.NET;

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

                foreach (var graph in report.Graphs)
                {
                    document.InsertParagraph(graph.Title).Bold();
                    document.InsertChart(graph.Data);
                    document.InsertParagraph();
                }

                document.InsertParagraph("9. Результаты");
                var table = document.AddTable(report.Results.Count, 5);
                table.Design = TableDesign.TableGrid;
                table.Alignment = Alignment.center;
                for (int i = 0; i < report.Results.Count; i++)
                {
                    table.Rows[i].Cells[0].Paragraphs.First().InsertText(report.Results[i].Num.ToString());
                    table.Rows[i].Cells[0].Paragraphs.First().InsertText(report.Results[i].Formula);
                    table.Rows[i].Cells[0].Paragraphs.First().InsertText(report.Results[i].NearProbe.ToString());
                    table.Rows[i].Cells[0].Paragraphs.First().InsertText(report.Results[i].FarProbe.ToString());
                    table.Rows[i].Cells[0].Paragraphs.First().InsertText(report.Results[i].FarToNearProbeRatio.ToString());
                }
                document.InsertTable(table);
                document.InsertParagraph();

                document.InsertParagraph("10. Выводы");
                document.InsertParagraph(report.Conclusion);

                document.Save();
            }
        }
    }
}
