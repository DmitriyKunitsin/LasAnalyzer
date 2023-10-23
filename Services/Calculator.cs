using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LasAnalyzer.Services
{
    public class Calculator
    {
        public double FindBaselineValue(List<double> data)
        {
            // Находим базовое значение, например, как среднее значение всего массива данных.
            return data.Average();
        }

        public (double, double, double, double) FindMinMaxTemperature(List<double> data, double baseline)
        {
            // Находим максимум и минимум, а также соответствующие температуры.
            double max = data.Max();
            double min = data.Min();

            // Проверяем, выходят ли максимум и минимум за порог 0.5% от базового значения.
            double threshold = 0.005 * baseline;

            if (Math.Abs(max - baseline) < threshold)
                max = baseline;

            if (Math.Abs(min - baseline) < threshold)
                min = baseline;

            return (max, min, GetTemperatureForValue(data, max), GetTemperatureForValue(data, min));
        }

        public double CalculateDeviation(double value, double baseline)
        {
            // Находим отклонение в процентах максимума или минимума от базового значения.
            return (value - baseline) / baseline * 100;
        }

        private double GetTemperatureForValue(List<double> data, double value)
        {
            // Находим соответствующую температуру для данного значения.
            int index = data.IndexOf(value); // Используйте подходящий метод для поиска значения в массиве.
            // Верните соответствующую температуру на этом индексе.
            return index >= 0 && index < data.Count ? GetTemperatureForIndex(index) : 0;
        }

        private double GetTemperatureForIndex(int index)
        {
            // Верните соответствующую температуру для заданного индекса.
            // Замените это заглушкой на вашу логику.
            return 0.0;
        }
    }
}
