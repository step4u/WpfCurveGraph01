using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
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
using Microsoft.Win32;
using SkiaSharp;
using System.IO;

namespace TestCurve01
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<ObservablePoint> points;

        public MainWindow()
        {
            InitializeComponent();


            points = new List<ObservablePoint>
            {
                new ObservablePoint(0, 0),
                new ObservablePoint(255, 255)
            };

            // 차트 설정
            curveChart.Series = new ISeries[]
            {
                new LineSeries<ObservablePoint>
                {
                    Values = points,
                    Fill = null,
                    LineSmoothness = 1
                }
            };

            curveChart.XAxes = new Axis[]
            {
                new Axis
                {
                    MinLimit = 0,
                    MaxLimit = 255
                }
            };

            curveChart.YAxes = new Axis[]
            {
                new Axis
                {
                    MinLimit = 0,
                    MaxLimit = 255
                }
            };
        }

        private void ApplyCurve_Click(object sender, RoutedEventArgs e)
        {
            // 이미지 불러오기
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (openFileDialog.ShowDialog() == true)
            {
                using var inputStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                var bitmap = SKBitmap.Decode(inputStream);
                var adjustedBitmap = ApplyCurveFilter(bitmap);
                DisplayImage(adjustedBitmap);
            }
        }

        private SKBitmap ApplyCurveFilter(SKBitmap original)
        {
            var curve = GetCurveMap();

            // 이미지의 모든 픽셀을 조정
            var adjusted = new SKBitmap(original.Width, original.Height);
            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    var color = original.GetPixel(x, y);

                    // R, G, B 각각의 값을 곡선에 따라 조정
                    byte r = curve[color.Red];
                    byte g = curve[color.Green];
                    byte b = curve[color.Blue];

                    adjusted.SetPixel(x, y, new SKColor(r, g, b, color.Alpha));
                }
            }
            return adjusted;
        }

        private byte[] GetCurveMap()
        {
            var curveMap = new byte[256];

            for (int i = 0; i < 256; i++)
            {
                double? value = i; // 초기 값은 double로 계산

                // 포인트 사이의 보간 수행
                for (int j = 0; j < points.Count - 1; j++)
                {
                    var (p1, p2) = (points[j], points[j + 1]);

                    if (value >= p1.X && value <= p2.X)
                    {
                        // 보간 비율 (t) 계산
                        double? t = (value - p1.X) / (p2.X - p1.X);

                        // 보간된 y 값 계산
                        value = p1.Y + t * (p2.Y - p1.Y);
                        break;
                    }
                }

                // double 값을 0~255 범위로 제한 후 byte로 변환
                curveMap[i] = (byte)Math.Clamp((int)value!, 0, 255);
            }

            return curveMap;
        }

        private void DisplayImage(SKBitmap bitmap)
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = new MemoryStream(data.ToArray());

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            imageDisplay.Source = bitmapImage;
        }


    }
}