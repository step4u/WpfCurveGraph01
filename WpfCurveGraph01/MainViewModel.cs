using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Windows;
using System.Diagnostics;
using LiveChartsCore.Measure;
using System.Windows.Media;

namespace WpfCurveGraph01
{
    internal partial class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            Init();
        }

        private KeyValuePair<int, string>? selectedItemHistoCombo = null;
        public KeyValuePair<int, string>? SelectedItemHistoCombo
        {
            get => selectedItemHistoCombo;
            set { SetProperty(ref selectedItemHistoCombo, value); }
        }

        private Dictionary<int, string>? histoComboItems = null;
        public Dictionary<int, string>? HistoComboItems
        {
            get => histoComboItems;
            set { SetProperty(ref histoComboItems, value); }
        }

        private ISeries[]? seriesHisto = null;
        public ISeries[]? SeriesHisto
        {
            get {  return seriesHisto; }
            set { SetProperty(ref seriesHisto, value); }
        }

        public DrawMarginFrame DrawMarginFrameHisto => new()
        {
            Fill = new SolidColorPaint(new SKColor(220, 220, 220)),
            Stroke = new SolidColorPaint(new SKColor(180, 180, 180), 1)
            //Fill = new SolidColorPaint(SKColors.CornflowerBlue),
            //Stroke = new SolidColorPaint(SKColors.CornflowerBlue, 1)
        };

        public Margin DrawMarginCurveHistogram => new Margin(0, 0, 0, 0);

        public DrawMarginFrame DrawMarginFrameCurve => new()
        {
            Fill = new SolidColorPaint(SKColors.Transparent),
            Stroke = new SolidColorPaint(SKColors.Transparent)
        };

        private ISeries[]? seriesCurve = null;
        public ISeries[]? SeriesCurve
        {
            get { return seriesCurve; }
            set
            {
                var chk = SetProperty(ref seriesCurve, value);
            }
        }

        public Axis[] XAxesHisto { get; set; }

        public Axis[] YAxesHisto { get; set; }

        public Axis[] XAxesCurve { get; set; }

        public Axis[] YAxesCurve { get; set; }


        private void Init()
        {
            XAxesHisto = new Axis[]
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

            YAxesHisto = new Axis[]
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

            XAxesCurve = new Axis[]
            {
                new Axis
                {
                    IsVisible = false,
                    MinLimit = 0,
                    LabelsPaint = new SolidColorPaint(SKColors.Transparent),
                    Labels = new List<string>(),
                    AnimationsSpeed = TimeSpan.Zero
                },
            };

            YAxesCurve = new Axis[]
            {
                new Axis
                {
                    IsVisible = false,
                    MinLimit = 0,
                    LabelsPaint = new SolidColorPaint(SKColors.Transparent),
                    Labels = new List<string>(),
                    AnimationsSpeed = TimeSpan.Zero
                },
            };

            HistoComboItems = new Dictionary<int, string>
            {
                { -1, "RGB" },
                { 2, "RED" },
                { 1, "GREEN" },
                { 0, "BLUE" },
            };

            selectedItemHistoCombo = HistoComboItems.FirstOrDefault();
        }

        private WriteableBitmap? selectedBitmap = null;

        [RelayCommand]
        private void OpenImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    BitmapImage bitmapImage = new BitmapImage(new Uri(openFileDialog.FileName));
                    WriteableBitmap writableBitmap = new WriteableBitmap(bitmapImage);

                    selectedBitmap = writableBitmap;

                    //var redHistogram = GetHistogram(writableBitmap, 2);
                    //var greenHistogram = GetHistogram(writableBitmap, 1);
                    //var blueHistogram = GetHistogram(writableBitmap, 0);

                    //var _series = new ISeries[]
                    //{
                    //    new ColumnSeries<int> { Values = redHistogram },
                    //    new ColumnSeries<int> { Values = greenHistogram },
                    //    new ColumnSeries<int> { Values = blueHistogram }
                    //};

                    UpdateHistogram();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}");
                }
            }
        }

        internal void SelectionChangedCurveCombo(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateHistogram();
        }

        private void UpdateHistogram()
        {
            if (selectedBitmap == null) return;

            ISeries[]? _series = null;

            if (SelectedItemHistoCombo != null)
            {
                if (SelectedItemHistoCombo.Value.Key == -1)
                {
                    var redHistogram = GetHistogram(selectedBitmap, 2);
                    var greenHistogram = GetHistogram(selectedBitmap, 1);
                    var blueHistogram = GetHistogram(selectedBitmap, 0);

                    //int maxVal = Math.Max(redHistogram.Max(), greenHistogram.Max());
                    //maxVal = Math.Max(maxVal, blueHistogram.Max());

                    _series = new ISeries[]
                    {
                        new LineSeries<int> { Values = redHistogram, Fill = new SolidColorPaint(SKColors.CornflowerBlue), Stroke = null, GeometryFill = null, GeometryStroke = null, IsHoverable = false },
                        new LineSeries<int> { Values = greenHistogram, Fill = new SolidColorPaint(SKColors.CornflowerBlue), Stroke = null, GeometryFill = null, GeometryStroke = null, IsHoverable = false },
                        new LineSeries<int> { Values = blueHistogram, Fill = new SolidColorPaint(SKColors.CornflowerBlue), Stroke = null, GeometryFill = null, GeometryStroke = null, IsHoverable = false },
                    };
                }
                else
                {
                    var histogram = GetHistogram(selectedBitmap, SelectedItemHistoCombo.Value.Key);

                    _series = new ISeries[]
                    {
                        new LineSeries<int> { Values = histogram, Fill = GetColorPaint(SelectedItemHistoCombo.Value.Key), Stroke = null, GeometryFill = null, GeometryStroke = null },
                    };
                }
            }

            SeriesHisto = _series;
        }


        private int[] GetHistogram(WriteableBitmap bitmap, int colorChannel)
        {
            string pixelFormat = GetPixelFormat(bitmap);
            int[] histogram = new int[256];
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = bitmap.BackBufferStride;
            byte[] pixels = new byte[height * stride];

            bitmap.CopyPixels(pixels, stride, 0);

            int colorOffset = GetColorOffset(pixelFormat, colorChannel);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * stride + x * 4;
                    byte colorValue = pixels[index + colorOffset];
                    histogram[colorValue]++;
                }
            }

            int minValue = histogram.Min();
            int maxValue = histogram.Max();


            for (int i = 0; i < histogram.Length; i++)
            {
                //histogram[i] = histogram[i] / 1000;
                histogram[i] = (int)(((double)(histogram[i] - minValue) / (maxValue - minValue)) * 255);
            }

            return histogram;
        }

        private SolidColorPaint GetColorPaint(int colorOffset)
        {
            SolidColorPaint solidColor = new SolidColorPaint(SKColors.Red);

            switch (colorOffset)
            {
                case 0:
                    solidColor = new SolidColorPaint(SKColors.Blue);
                    break;
                case 1:
                    solidColor = new SolidColorPaint(SKColors.Green);
                    break;
                default:
                case 2:
                    solidColor = new SolidColorPaint(SKColors.Red);
                    break;
            }

            return solidColor;
        }

        private string GetPixelFormat(WriteableBitmap bitmap)
        {
            var pixelFormat = bitmap.Format;

            return pixelFormat == PixelFormats.Bgra32 ? "BGRA" :
                   pixelFormat == PixelFormats.Pbgra32 ? "pBGRA" : // 알파가 프리멀티플라이된 경우
                   pixelFormat == PixelFormats.Rgb24 ? "RGB" :
                   pixelFormat == PixelFormats.Rgba64 ? "RGBA64" :
                   pixelFormat == PixelFormats.Prgba64 ? "pRGBA64" :
                   pixelFormat == PixelFormats.Gray8 ? "GRAY8" :
                   pixelFormat == PixelFormats.Bgr24 ? "BGR" :
                   pixelFormat == PixelFormats.Bgr32 ? "BGR32" :
                   pixelFormat == PixelFormats.Rgba128Float ? "RGBA128" :
                   "UNKNOWN";
        }

        private int GetColorOffset(string pixelFormat, int colorChannel)
        {
            return pixelFormat switch
            {
                "ARGB" => colorChannel switch
                {
                    0 => 1, // Red
                    1 => 2, // Green
                    2 => 3, // Blue
                    3 => 0, // Alpha
                    _ => throw new ArgumentException("잘못된 색상 채널입니다.")
                },
                "BGRA" or "BGR32" => colorChannel switch
                {
                    0 => 2, // Red
                    1 => 1, // Green
                    2 => 0, // Blue
                    3 => 3, // Alpha
                    _ => throw new ArgumentException("잘못된 색상 채널입니다.")
                },
                "RGB" => colorChannel switch
                {
                    0 => 0, // Red
                    1 => 1, // Green
                    2 => 2, // Blue
                    _ => throw new ArgumentException("RGB 포맷에서는 알파 채널을 사용할 수 없습니다.")
                },
                _ => throw new ArgumentException("지원되지 않는 픽셀 포맷입니다.")
            };
        }

    }
}
