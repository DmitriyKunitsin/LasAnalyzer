using OxyPlot.Series;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace LasAnalyzer.Services
{
    public class GraphService
    {
        public PlotModel OxyPlotModel { get; private set; }
        public List<Chart> XceedCharts { get; private set; }

        public GraphService()
        {
            // Инициализация OxyPlot модели и данных
            OxyPlotModel = new PlotModel();
            var series = new LineSeries();
            // Заполните series данными для графика
            OxyPlotModel.Series.Add(series);

            // Инициализация коллекции для Xceed графиков
            XceedCharts = new List<Chart>();
        }

        public void GenerateOxyPlot()
        {
            // Генерация графика OxyPlot
            // Заполнение OxyPlot данными и настройка внешнего вида
        }

        public void GenerateXceedCharts()
        {
            // Генерация графиков для Xceed.Words.NET
            //foreach (var data in XceedChartData)
            //{
            //    var chart = new Chart();
            //    // Заполнение chart данными и настройка внешнего вида
            //    XceedCharts.Add(chart);
            //}
        }

        public void InsertChartsInDocument(DocX document)
        {
            // Вставка графиков в документ с использованием Xceed.Words.NET
            foreach (var chart in XceedCharts)
            {
                // Вставка chart в документ
                document.InsertChart(chart);
            }
        }
    }
}
