using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LasAnalyzer.Services
{
    public class ChartProcessor
    {
        public List<double> smoothData(List<double> Data)
        {
            return Data;
        }
        public PlotModel CreateLineChart(List<double> xData, List<double> yData, string title, string xTitle, string yTitle)
        {
            var model = new PlotModel { Title = title };
            var series = new LineSeries();

            for (int i = 0; i < xData.Count; i++)
            {
                series.Points.Add(new DataPoint(xData[i], yData[i]));
            }

            model.Series.Add(series);
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = xTitle });
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = yTitle });

            return model;
        }

        public PlotModel ResizeChart(PlotModel chart, double xMin, double xMax, double yMin, double yMax)
        {
            // Устанавливаем границы для осей графика.
            foreach (var axis in chart.Axes)
            {
                if (axis is LinearAxis linearAxis)
                {
                    if (axis.Position == AxisPosition.Bottom)
                    {
                        linearAxis.Minimum = xMin;
                        linearAxis.Maximum = xMax;
                    }
                    else if (axis.Position == AxisPosition.Left)
                    {
                        linearAxis.Minimum = yMin;
                        linearAxis.Maximum = yMax;
                    }
                }
            }

            return chart;
        }

        public PlotModel CropChart(PlotModel chart, double xMin, double xMax, double yMin, double yMax)
        {
            // Убираем данные, не попадающие в заданный диапазон.
            var lineSeries = chart.Series[0] as LineSeries;
            if (lineSeries != null)
            {
                var newPoints = lineSeries.Points.Where(p => p.X >= xMin && p.X <= xMax && p.Y >= yMin && p.Y <= yMax).ToList();
                lineSeries.Points.Clear();
                lineSeries.Points.AddRange(newPoints);
            }

            return chart;
        }
    }
}
