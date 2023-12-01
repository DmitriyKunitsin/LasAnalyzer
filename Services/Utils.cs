using LasAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LasAnalyzer.Services
{
    public static class Utils
    {
        public static double GetStepForSeparators(double maxMinDiff)
        {
            // если по Y должны быть дробные значения, округляем до 2 знаков после точки
            double step = maxMinDiff >= 10 ? Math.Round(maxMinDiff / 4) : Math.Round(maxMinDiff / 4, 2);

            if (step >= 500 && step % 500 != 0)
                step = Math.Round(step / 500) * 500;
            else if (step >= 100 && step % 100 != 0)
                step = Math.Round(step / 100) * 100;
            else if (step >= 50 && step % 50 != 0)
                step = Math.Round(step / 50) * 50;
            else if (step >= 25 && step % 25 != 0)
                step = Math.Round(step / 25) * 25;
            else if (step >= 10 && step % 10 != 0)
                step = Math.Round(step / 10) * 10;
            else if (step >= 5 && step % 5 != 0)
                step = Math.Round(step / 5) * 5;

            if (step < 5)
            {
                step = step * 100;
                if (step % 10 != 0)
                    step = Math.Round(step / 10) * 10;
                else if (step % 5 != 0)
                    step = Math.Round(step / 5) * 5;

                step = step / 100;
            }

            return step;
        }
    }
}
