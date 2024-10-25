using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Telerik.Windows.Data;

namespace WpfCurveGraph02
{
    public partial class MainViewModel : ObservableObject
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

        private int[] seriesHisto;
        public int[] SeriesHisto
        {
            get => seriesHisto;
            set { SetProperty(ref seriesHisto, value); }
        }

        private int[] seriesCurveGuide;
        public int[] SeriesCurveGuide
        {
            get => seriesCurveGuide;
            set { SetProperty(ref seriesCurveGuide, value); }
        }

        private ObservableCollection<Point> seriesCurve;
        public ObservableCollection<Point> SeriesCurve
        {
            get => seriesCurve;
            set { SetProperty(ref seriesCurve, value); }
        }

        private ObservableCollection<Point> seriesScatter;
        public ObservableCollection<Point> SeriesScatter
        {
            get => seriesScatter;
            set { SetProperty(ref seriesScatter, value); }
        }

        private WriteableBitmap? selectedBitmap = null;
        private byte[]? selectedBitmapBuffer = null;

        public byte[]? SelectedImageBuffer
        {
            get => selectedBitmapBuffer;
        }

        private BitmapImage? selectedImageSource = null;
        public BitmapImage? SelectedImageSource
        {
            get => selectedImageSource;
            set { SetProperty(ref selectedImageSource, value); }
        }

        private void Init()
        {
            HistoComboItems = new Dictionary<int, string>
            {
                { -1, "RGB" },
                { 2, "RED" },
                { 1, "GREEN" },
                { 0, "BLUE" },
            };

            selectedItemHistoCombo = HistoComboItems.FirstOrDefault();

            List<int> curvePoints = new();
            for (int i = 0; i < 256; i++)
                curvePoints.Add(i);

            seriesCurveGuide = curvePoints.ToArray();
            SeriesCurve = new(curvePoints.Select((x, index) =>
            {
                return new Point(index, x);
            }));

            //Random ran = new Random();

            //ObservableCollection<Point> curves = new ObservableCollection<Point>();

            //for (int i = 0; i < 256; i++)
            //{
            //    curves.Add(new Point(i, ran.Next(0, 255)));
            //}

            //SeriesCurve = curves;

            SeriesScatter = new ObservableCollection<Point>()
            {
                { new Point(0, 0) },
                { new Point(128, 128) },
                { new Point(255, 255) }
            };
        }

        [RelayCommand]
        private void OpenImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var fs = File.Open(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        selectedBitmapBuffer = new byte[fs.Length];
                        fs.Read(selectedBitmapBuffer, 0, selectedBitmapBuffer.Length);
                    }

                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = new MemoryStream(selectedBitmapBuffer);
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    WriteableBitmap writableBitmap = new WriteableBitmap(bitmapImage);

                    selectedBitmap = writableBitmap;
                    SelectedImageSource = bitmapImage;

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

            if (SelectedItemHistoCombo != null)
            {
                if (SelectedItemHistoCombo.Value.Key == -1)
                {
                    var histo = GetHistogramRGB(selectedBitmap);
                    SeriesHisto = histo;
                }
                else
                {
                    var histo = GetHistogram(selectedBitmap, SelectedItemHistoCombo.Value.Key);
                    SeriesHisto = histo;
                }
            }
        }


        private int[] GetHistogramRGB(WriteableBitmap bitmap)
        {
            string pixelFormat = GetPixelFormat(bitmap);
            int[] histogram = new int[256];
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = bitmap.BackBufferStride;
            byte[] pixels = new byte[height * stride];

            bitmap.CopyPixels(pixels, stride, 0);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * stride + x * 4;

                    // Red, Green, Blue 값 추출
                    byte blue = pixels[index];       // Blue 채널
                    byte green = pixels[index + 1];  // Green 채널
                    byte red = pixels[index + 2];    // Red 채널

                    // RGB 값을 평균으로 통합
                    byte avgColorValue = (byte)((red + green + blue) / 3);

                    // 히스토그램 값 증가
                    histogram[avgColorValue]++;
                }
            }

            int minValue = histogram.Min();
            int maxValue = histogram.Max();
            double range = maxValue - minValue;
            if (range == 0) range = 1;

            for (int i = 0; i < histogram.Length; i++)
            {
                histogram[i] = (int)(((double)(histogram[i] - minValue) / range) * 255);
            }

            return histogram;
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
            double range = maxValue - minValue;
            if (range == 0) range = 1;

            for (int i = 0; i < histogram.Length; i++)
            {
                histogram[i] = (int)(((double)(histogram[i] - minValue) / range) * 255);
            }

            return histogram;
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

        //private SolidColorPaint GetColorPaint(int colorOffset)
        //{
        //    SolidColorPaint solidColor = new SolidColorPaint(SKColors.Red);

        //    switch (colorOffset)
        //    {
        //        case 0:
        //            solidColor = new SolidColorPaint(SKColors.Blue);
        //            break;
        //        case 1:
        //            solidColor = new SolidColorPaint(SKColors.Green);
        //            break;
        //        default:
        //        case 2:
        //            solidColor = new SolidColorPaint(SKColors.Red);
        //            break;
        //    }

        //    return solidColor;
        //}

    }
}
