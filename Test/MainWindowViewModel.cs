using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    internal class MainWindowViewModel: ObservableObject
    {
        Random ran = new Random();

        public MainWindowViewModel()
        {
            XAxes = new Axis[]
            {
                new Axis
                {
                    IsVisible = true,
                    MinLimit = 0,
                    MaxLimit = 255,
                    LabelsPaint = new SolidColorPaint(SKColors.Transparent),
                    Labels = new List<string>(),
                    AnimationsSpeed = TimeSpan.Zero
                },
            };

            YAxes = new Axis[]
            {
                new Axis
                {
                    IsVisible = false,
                    MinLimit = 0,
                    MaxLimit = 255,
                    LabelsPaint = new SolidColorPaint(SKColors.Transparent),
                    Labels = new List<string>(),
                    AnimationsSpeed = TimeSpan.Zero
                },
            };


            int[] seriesValues = new int[256];

            for (int i = 0; i < seriesValues.Length; i++)
            {
                var rnum = (int)ran.NextInt64(0, 255);
                seriesValues[i] = rnum;
            }


            var _series = new ISeries[]
            {
                new LineSeries<int>
                {
                    Values = seriesValues,
                    Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                    Stroke = null,
                    GeometryFill = null,
                    GeometryStroke = null
                }
            };

            Series = _series;
        }

        public ISeries[] series;
        public ISeries[] Series
        {
            get => series;
            set { SetProperty(ref series, value); }
        }


        public Axis[] XAxes { get; set; }

        public Axis[] YAxes { get; set; }

        // Creates a gray background and border in the draw margin.
        public DrawMarginFrame DrawMarginFrame => new()
        {
            Fill = new SolidColorPaint(new SKColor(220, 220, 220)),
            Stroke = new SolidColorPaint(new SKColor(180, 180, 180), 1)
        };
    }
}
