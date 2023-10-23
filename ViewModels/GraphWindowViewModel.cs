using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LasAnalyzer.ViewModels
{
    public class GraphWindowViewModel : ViewModelBase
    {
        public PlotModel PlotModel1 { get; }
        public PlotModel PlotModel2 { get; }
        // Add more PlotModels for your other graphs

        public GraphWindowViewModel()
        {
            PlotModel1 = new PlotModel { Title = "Graph 1" };
            PlotModel2 = new PlotModel { Title = "Graph 2" };
            // Initialize and configure your PlotModels, add series, set axes, etc.
        }
    }
}
