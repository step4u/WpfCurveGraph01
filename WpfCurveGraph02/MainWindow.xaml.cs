using MathNet.Numerics.Interpolation;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using Telerik.Charting;
using Telerik.Windows.Controls;
using Telerik.Windows.Persistence.Core;

namespace WpfCurveGraph02
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ClosestPointFinder finder = new ClosestPointFinder();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox control = (ComboBox)sender;

            if (!control.IsMouseOver)
                return;

            vmodel.SelectionChangedCurveCombo(sender, e);
        }

        bool HasNearPoint = false;
        Point selectedScatter = new Point(-1, -1);

        private void RadCartesianChart_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            var chart = sender as RadCartesianChart;
            if (chart == null) return;

            var channel = vmodel.SelectedItemHistoCombo;
            var curveData = vmodel.SeriesCurve;
            var scatterData = vmodel.SeriesScatter;

            if (curveData != null)
            {
                var chartPos = e.GetPosition(chart);
                var pos = new Point(
                    (int)(Math.Min(1, Math.Max(0, (chartPos.X / chart.ActualWidth))) * 255),
                    (int)(Math.Min(1, Math.Max(0, ((chart.ActualHeight - chartPos.Y) / chart.ActualHeight))) * 255));

                finder.PointCollections = scatterData;
                selectedScatter = finder.FindClosestPoint(pos);

                curveIsDragging = true;
                chart.CaptureMouse();
            }
        }
        
        private void RadCartesianChart_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var chart = sender as RadCartesianChart;
            if (chart == null) return;

            var chartPos = e.GetPosition(chart);
            var curPoint = new Point(
                (int)(Math.Min(1, Math.Max(0, (chartPos.X / chart.ActualWidth))) * 255),
                (int)(Math.Min(1, Math.Max(0, ((chart.ActualHeight - chartPos.Y) / chart.ActualHeight))) * 255));


            var curveData = vmodel.SeriesCurve;
            var scatterData = vmodel.SeriesScatter;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!curveIsDragging) return;

                Mouse.OverrideCursor = Cursors.Cross;

                HasNearPoint = true;

                var idx = scatterData.IndexOf(selectedScatter);
                if (idx > -1)
                {
                    scatterData[idx] = curPoint;


                    UpdateCurve(scatterData, curveData);

                    //UpdateCurve2(selectedScatter, curPoint);

                    selectedScatter = curPoint;
                }
            }
            else
            {
                bool isNearScatterPoint = scatterData.Any(point => Math.Abs(point.X - curPoint.X) <= 5 && Math.Abs(point.Y - curPoint.Y) <= 5);

                if (isNearScatterPoint)
                {
                    Mouse.OverrideCursor = Cursors.ScrollAll;

                    HasNearPoint = true;
                }
                else
                {
                    bool isNearPoint = curveData.Any(point => Math.Abs(point.X - curPoint.X) <= 5 && Math.Abs(point.Y - curPoint.Y) <= 5);

                    if (isNearPoint)
                    {
                        Mouse.OverrideCursor = Cursors.Cross;

                        HasNearPoint = true;
                    }
                    else
                    {
                        Mouse.OverrideCursor = null;
                    }
                }
            }
        }

        bool curveIsDragging = false;
        private void RadCartesianChart_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var chart = sender as RadCartesianChart;
            if (chart == null) return;

            chart.ReleaseMouseCapture();

            if (e.LeftButton != MouseButtonState.Pressed) return;

            if (!curveIsDragging) return;

            selectedScatter = new Point(-1, -1);
            curveIsDragging = false;
            HasNearPoint = false;
        }

        private void btnGetPoints_Click(object sender, RoutedEventArgs e)
        {
            var scatterData = vmodel.SeriesScatter;

            List<Point> interpolatedPoints = GetSplinePoints(scatterData);
            foreach (var point in interpolatedPoints)
            {
                Console.WriteLine($"X: {point.X}, Y: {point.Y}");
            }
        }


        private List<Point> GetSplinePoints(ObservableCollection<Point> data)
        {
            List<Point> splinePoints = new List<Point>();

            // 데이터 포인트의 최소, 최대값을 찾음 (정규화 위해)
            double minX = 0, maxX = 255, minY = 0, maxY = 255;

            // 총 255개의 보간된 점을 생성하기 위해 각 구간의 세그먼트 계산
            int totalSegments = 255 / (data.Count - 1);  // 각 구간에 할당된 세그먼트 수

            for (int i = 0; i < data.Count - 1; i++)
            {
                var p0 = i > 0 ? data[i - 1] : data[i];
                var p1 = data[i];
                var p2 = data[i + 1];
                var p3 = i < data.Count - 2 ? data[i + 2] : data[i + 1];

                for (int j = 0; j < totalSegments; j++)
                {
                    double t = j / (double)totalSegments;
                    double tt = t * t;
                    double ttt = tt * t;

                    // Catmull-Rom 스플라인 공식
                    double x = 0.5 * ((2 * p1.X) +
                                     (-p0.X + p2.X) * t +
                                     (2 * p0.X - 5 * p1.X + 4 * p2.X - p3.X) * tt +
                                     (-p0.X + 3 * p1.X - 3 * p2.X + p3.X) * ttt);

                    double y = 0.5 * ((2 * p1.Y) +
                                     (-p0.Y + p2.Y) * t +
                                     (2 * p0.Y - 5 * p1.Y + 4 * p2.Y - p3.Y) * tt +
                                     (-p0.Y + 3 * p1.Y - 3 * p2.Y + p3.Y) * ttt);

                    // 계산된 값을 (0,0) ~ (255,255) 범위로 정규화
                    int normalizedX = (int)Math.Clamp((x / maxX) * 255, 0, 255);
                    int normalizedY = (int)Math.Clamp((y / maxY) * 255, 0, 255);

                    splinePoints.Add(new Point(normalizedX, normalizedY));

                    // 255개의 점만 생성되도록 조기 종료
                    if (splinePoints.Count >= 255)
                        return splinePoints;
                }
            }

            // 만약 정확히 255개가 생성되지 않았다면 남은 점을 추가
            while (splinePoints.Count < 255)
            {
                splinePoints.Add(new Point(255, 255)); // 마지막 점을 채움
            }

            return splinePoints;
        }

        private void UpdateCurve(ObservableCollection<Point> scatters, ObservableCollection<Point> curves)
        {
            double[] xValues = scatters.Select(p => (double)p.X).ToArray();
            double[] yValues = scatters.Select(p => (double)p.Y).ToArray();

            var spline = CubicSpline.InterpolateNatural(xValues, yValues);

            var integerPoints = new ObservableCollection<Point>();

            for (int x = 0; x <= 255; x++)
            {
                double y = spline.Interpolate(x);
                integerPoints.Add(new Point(x, (int)y));
                curves[x] = new Point(x, (int)y);
            }

            //vmodel.SeriesCurve = integerPoints;
        }

        private void UpdateCurve2(Point oPoint, Point nPoint)
        {
            var idx = vmodel.SeriesCurve.IndexOf(oPoint);
            vmodel.SeriesCurve[idx] = nPoint;
        }

    }
}