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
        // todo: find temper max with index
        public static int? FindIndexForBaseValue(List<double> tempData, TempType tempType, int windowSize)
        {
            if (tempType == TempType.Heating)
            {
                var tBaseMaxIndex = FindTemperatureRisePoint(tempData, searchLeft: true);
                if (tBaseMaxIndex != null)
                {
                    return Math.Min(tBaseMaxIndex.Value, windowSize * 5);
                }
            }
            else if (tempType == TempType.Cooling)
            {
                var tBaseMaxIndex = FindTemperatureRisePoint(tempData, searchLeft: false);
                if (tBaseMaxIndex != null)
                {
                    if ((tempData.Count - 1) - tBaseMaxIndex < windowSize * 5)
                    {
                        return tBaseMaxIndex.Value;
                    }
                    else
                    {
                        return (tempData.Count - 1) - windowSize * 5;
                    }
                }
            }
            return null;
        }

        public static int? FindTemperatureRisePoint(List<double> tempData, bool searchLeft)
        {
            int start = searchLeft ? 0 : tempData.Count - 1;
            int step = searchLeft ? 1 : -1;

            for (int i = start; searchLeft ? i < tempData.Count - 1 : i > 0; i += step)
            {
                double tempDifference = searchLeft ? tempData[i + 1] - tempData[i] : tempData[i - 1] - tempData[i];

                if (tempDifference >= 1)
                {
                    return i;
                }
            }

            return null;
        }
    }
}
