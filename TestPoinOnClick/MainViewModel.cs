using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.Drawing;

namespace TestPoinOnClick
{
    internal partial class MainViewModel : ObservableObject
    {

        public ISeries[] SeriesCollection { get; set; } =
        new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = new ObservableCollection<ObservablePoint>
                {
                    new(0, 5),
                    new(3, 8),
                    new(7, 9)
                },
                Fill = null,
                DataPadding = new LiveChartsCore.Drawing.LvcPoint(5, 5)
            }
        };

        [RelayCommand]
        public void PointerDown(PointerCommandArgs args)
        {
            var chart = (ICartesianChartView<SkiaSharpDrawingContext>)args.Chart;
            var values = (ObservableCollection<ObservablePoint>)SeriesCollection[0].Values!;

            // scales the UI coordinates to the corresponding data in the chart.
            var scaledPoint = chart.ScalePixelsToData(args.PointerPosition);

            // finally add the new point to the data in our chart.
            values.Add(new ObservablePoint(scaledPoint.X, scaledPoint.Y));
        }
    }
}
