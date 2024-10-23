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
        public ISeries[] Series { get; set; } =
    {
        new LineSeries<double>
        {
            Values = new double[] { 0, 3, 5, 3, 4, 6 },
            Fill = new SolidColorPaint(SKColors.CornflowerBlue),
            Stroke = null,
            GeometryFill = null,
            GeometryStroke = null
        }
    };

        // Creates a gray background and border in the draw margin.
        public DrawMarginFrame DrawMarginFrame => new()
        {
            Fill = new SolidColorPaint(new SKColor(220, 220, 220)),
            Stroke = new SolidColorPaint(new SKColor(180, 180, 180), 1)
        };
    }
}
