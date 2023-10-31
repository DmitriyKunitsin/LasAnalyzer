using LasAnalyzer.Models;
using LiveChartsCore;
using LiveChartsCore.Geo;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LasAnalyzer.Services
{
    public class SeriesData
    {
        public ISeries[] NearProbeSeries { get; set; }
        public ISeries[] FarProbeSeries { get; set; }
        public ISeries[] FarToNearProbeRatioSeries { get; set; }
        public ISeries[] TemperatureSeries { get; set; }

        public SeriesData()
        {
            NearProbeSeries = new ISeries[]
            {
                new LineSeries<double>()
            };

            FarProbeSeries = new ISeries[]
            {
                new LineSeries<double>()
            };

            FarToNearProbeRatioSeries = new ISeries[]
            {
                new LineSeries<double>()
            };

            TemperatureSeries = new ISeries[]
            {
                new LineSeries<double>()
            };
        }

        public SeriesData(GraphData graphData)
        {
            NearProbeSeries = new ISeries[]
            {
                    new LineSeries<double>
                    {
                        Values = graphData.NearProbe,
                        GeometryStroke = null,
                        GeometryFill = null,
                        Fill = null,
                        Stroke = new SolidColorPaint
                        {
                            Color = SKColors.BlueViolet,
                            StrokeThickness = 3,
                            ZIndex = 1
                        },
                        ZIndex = 1,
                    }
                };
            FarProbeSeries = new ISeries[]
            {
                    new LineSeries<double>
                    {
                        Values = graphData.FarProbe,
                        GeometryStroke = null,
                        GeometryFill = null,
                        Fill = null,
                        Stroke = new SolidColorPaint
                        {
                            Color = SKColors.BlueViolet,
                            StrokeThickness = 3,
                        },
                        LineSmoothness = 0
                    }
            };

            FarToNearProbeRatioSeries = new ISeries[]
            {
                    new LineSeries<double>
                    {
                        Values = graphData.FarToNearProbeRatio,
                        GeometryStroke = null,
                        GeometryFill = null,
                        Fill = null,
                        Stroke = new SolidColorPaint
                        {
                            Color = SKColors.BlueViolet,
                            StrokeThickness = 3,
                            ZIndex = 1
                        }
                    }
            };
            TemperatureSeries = new ISeries[]
            {
                    new LineSeries<double>
                    {
                        Values = graphData.Temperature,
                        GeometryStroke = null,
                        GeometryFill = null,
                        Fill = null,
                        Stroke = new SolidColorPaint
                        {
                            Color = SKColors.BlueViolet,
                            StrokeThickness = 3,
                        }
                    }
            };
        }
    }
}
