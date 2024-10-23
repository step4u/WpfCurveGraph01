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
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.Painting;

namespace TestPoinOnClick
{
    internal partial class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            Init();
        }

        private void Init()
        {
            var values = new ObservableCollection<ObservablePoint>();
            for (int i = 0; i < 256; i++)
                values.Add(new ObservablePoint(i, i));

            SeriesCollection = new ISeries[]
            {
                new LineSeries<ObservablePoint>
                {
                    Values = values,
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColors.Black, 1),
                    DataPadding = new LiveChartsCore.Drawing.LvcPoint(0, 0),
                    LineSmoothness = 1,
                    GeometrySize = 6,
                    GeometryStroke = new SolidColorPaint(SKColors.Black, 1),
                }
            };
        }

        public ISeries[]? seriesCollection = null;
        public ISeries[]? SeriesCollection
        {
            get => seriesCollection;
            set { SetProperty(ref seriesCollection, value); }
        }

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
