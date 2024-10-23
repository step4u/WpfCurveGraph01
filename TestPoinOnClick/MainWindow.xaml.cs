using LiveChartsCore.SkiaSharpView.WPF;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestPoinOnClick
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        LiveChartsCore.Kernel.ChartPoint grapPoint;
        private void chart_ChartPointPointerDown(LiveChartsCore.Kernel.Sketches.IChartView chart, LiveChartsCore.Kernel.ChartPoint point)
        {
            var control = chart as CartesianChart;
            if (control == null) return;

            if (point == null)
            {
                
            }
            else
            {
                grapPoint = point;

                if (!control.IsMouseCaptured)
                    control.CaptureMouse();
            }
        }

        private void chart_MouseMove(object sender, MouseEventArgs e)
        {
            var control = sender as CartesianChart;
            if (control == null) return;

            if (control.IsMouseCaptured)
            {
                var point = e.GetPosition(control);
            }
        }

        private void chart_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var control = sender as CartesianChart;
            if (control == null) return;
            
            if (control.IsMouseCaptured)
                control.ReleaseMouseCapture();

        }
    }
}