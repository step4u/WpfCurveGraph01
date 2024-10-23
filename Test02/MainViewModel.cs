using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test02
{
    internal class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {

        }

        public ISeries[] Series { get; set; } =
        {
            new LineSeries<ObservablePoint>
            {
                Values = new ObservablePoint[]
                {
                    new ObservablePoint(0, 4),
                    new ObservablePoint(1, 3),
                    new ObservablePoint(3, 8),
                    new ObservablePoint(18, 6),
                    new ObservablePoint(20, 12)
                }
            }
        };
    }
}
