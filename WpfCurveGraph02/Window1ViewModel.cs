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
    public partial class Window1ViewModel : ObservableObject
    {
        public Window1ViewModel()
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

        private Brush areaColor = Brushes.Magenta;
        public Brush AreaColor
        {
            get => areaColor;
            set { SetProperty(ref areaColor, value); }
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

        internal ObservableCollection<Point> seriesCurveRGB;

        internal ObservableCollection<Point> seriesCurveR;

        internal ObservableCollection<Point> seriesCurveG;

        internal ObservableCollection<Point> seriesCurveB;

        
        private ObservableCollection<Point> seriesScatter;
        public ObservableCollection<Point> SeriesScatter
        {
            get => seriesScatter;
            set { SetProperty(ref seriesScatter, value); }
        }

        internal ObservableCollection<Point> seriesScatterRGB;

        internal ObservableCollection<Point> seriesScatterR;

        internal ObservableCollection<Point> seriesScatterG;

        internal ObservableCollection<Point> seriesScatterB;


        private WriteableBitmap? oriWritableBitmap = null;
        public WriteableBitmap? OriWritableBitmap
        {
            get => oriWritableBitmap;
            set { SetProperty(ref oriWritableBitmap, value); }
        }

        private WriteableBitmap? selectedWritableBitmap = null;
        public WriteableBitmap? SelectedWritableBitmap
        {
            get => selectedWritableBitmap;
            set { SetProperty(ref selectedWritableBitmap, value); }
        }

        private WriteableBitmap? layeredWritableBitmap = null;
        public WriteableBitmap? LayeredWritableBitmap
        {
            get => layeredWritableBitmap;
            set
            {
                SetProperty(ref layeredWritableBitmap, value);
            }
        }

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

        private byte[]? oriPixels = null;
        public byte[]? OriPixels
        {
            get => oriPixels;
            set { SetProperty(ref oriPixels, value); }
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

            SelectedItemHistoCombo = HistoComboItems.FirstOrDefault();

            List<int> curvePoints = new();
            for (int i = 0; i < 256; i++)
                curvePoints.Add(i);

            seriesCurveGuide = curvePoints.ToArray();
            seriesCurveRGB = new(curvePoints.Select((x, index) =>
            {
                return new Point(index, x);
            }));

            seriesCurveR = new(curvePoints.Select((x, index) =>
            {
                return new Point(index, x);
            }));

            seriesCurveG = new(curvePoints.Select((x, index) =>
            {
                return new Point(index, x);
            }));

            seriesCurveB = new(curvePoints.Select((x, index) =>
            {
                return new Point(index, x);
            }));

            SeriesCurve = seriesCurveRGB;

            //Random ran = new Random();
            //ObservableCollection<Point> curves = new ObservableCollection<Point>();

            //for (int i = 0; i < 256; i++)
            //{
            //    curves.Add(new Point(i, ran.Next(0, 255)));
            //}

            //SeriesCurve = curves;

            seriesScatterRGB = new ObservableCollection<Point>()
            {
                { new Point(0, 0) },
                { new Point(255, 255) }
            };

            seriesScatterR = new ObservableCollection<Point>()
            {
                { new Point(0, 0) },
                { new Point(255, 255) }
            };

            seriesScatterG = new ObservableCollection<Point>()
            {
                { new Point(0, 0) },
                { new Point(255, 255) }
            };

            seriesScatterB = new ObservableCollection<Point>()
            {
                { new Point(0, 0) },
                { new Point(255, 255) }
            };

            SeriesScatter = seriesScatterRGB;
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

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(selectedBitmapBuffer);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    SelectedWritableBitmap = new WriteableBitmap(bitmap);
                    //OriPixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
                    //bitmap.CopyPixels(OriPixels, bitmap.PixelWidth * 4, 0);

                    OriWritableBitmap = new WriteableBitmap(bitmap);

                    // 빈 값
                    //int width = oriWritableBitmap.PixelWidth;
                    //int height = oriWritableBitmap.PixelHeight;
                    //int stride = width * (oriWritableBitmap.Format.BitsPerPixel / 8);
                    //byte[] pixelData = new byte[height * stride];

                    //WriteableBitmap deepCopiedBitmap = new WriteableBitmap(width, height, oriWritableBitmap.DpiX, oriWritableBitmap.DpiY, oriWritableBitmap.Format, null);
                    ////WriteableBitmap deepCopiedBitmap = new WriteableBitmap(width, height, bitmap.DpiX, bitmap.DpiY, bitmap.Format, null);
                    //deepCopiedBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

                    //LayeredWritableBitmap = deepCopiedBitmap;

                    LayeredWritableBitmap = CreateTransparentWriteableBitmap(OriWritableBitmap.PixelWidth, OriWritableBitmap.PixelHeight, OriWritableBitmap.DpiX, OriWritableBitmap.DpiY);

                    UpdateHistogram();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}");
                }
            }
        }

        private WriteableBitmap CreateTransparentWriteableBitmap(int width, int height, double dpix, double dpiy)
        {
            // WriteableBitmap 생성: 너비, 높이, 해상도, 픽셀 포맷, 팔레트 (없으므로 null)
            WriteableBitmap bitmap = new WriteableBitmap(width, height, dpix, dpiy, PixelFormats.Bgra32, null);

            // 투명 배경을 위해 모든 픽셀을 0으로 설정
            //bitmap.Lock();
            //unsafe
            //{
            //    // 비트맵의 시작 위치를 가져옴
            //    IntPtr pBackBuffer = bitmap.BackBuffer;
            //    int stride = bitmap.BackBufferStride;

            //    // 투명하게 설정 (0으로 채움: BGRA 각각 0, 0, 0, 0)
            //    for (int y = 0; y < height; y++)
            //    {
            //        for (int x = 0; x < width; x++)
            //        {
            //            // 각 픽셀을 0으로 설정
            //            int pixelOffset = y * stride + x * 4;
            //            *((int*)(pBackBuffer + pixelOffset)) = 0x00000000;
            //        }
            //    }
            //}
            //bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            //bitmap.Unlock();


            int[] pixels = new int[width * height];
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);

            return bitmap;
        }

        [RelayCommand]
        private void InverseImage()
        {
            if (SelectedWritableBitmap == null) return;

            ImageUtil.InverseImage(SelectedWritableBitmap);
        }

        internal void SelectionChangedCurveCombo(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateHistogram();
        }

        private void UpdateHistogram()
        {
            if (SelectedWritableBitmap == null) return;

            if (SelectedItemHistoCombo != null)
            {
                if (SelectedItemHistoCombo.Value.Key == -1)
                {
                    var histo = GetHistogramRGB(SelectedWritableBitmap);
                    SeriesHisto = histo;
                    AreaColor = Brushes.LightGray;

                    SeriesCurve = seriesCurveRGB;
                    SeriesScatter = seriesScatterRGB;
                }
                else
                {
                    var histo = GetHistogram(SelectedWritableBitmap, SelectedItemHistoCombo.Value.Key);
                    SeriesHisto = histo;
                    AreaColor = SelectedItemHistoCombo.Value.Key == 2 ? Brushes.Pink
                        : SelectedItemHistoCombo.Value.Key == 1 ? Brushes.LightGreen
                        : SelectedItemHistoCombo.Value.Key == 0 ? Brushes.SkyBlue : Brushes.LightGray;

                    SeriesCurve = SelectedItemHistoCombo.Value.Key == 2 ? seriesCurveR
                        : SelectedItemHistoCombo.Value.Key == 1 ? seriesCurveG
                        : SelectedItemHistoCombo.Value.Key == 0 ? seriesCurveB : seriesCurveRGB;

                    SeriesScatter = SelectedItemHistoCombo.Value.Key == 2 ? seriesScatterR
                        : SelectedItemHistoCombo.Value.Key == 1 ? seriesScatterG
                        : SelectedItemHistoCombo.Value.Key == 0 ? seriesScatterB : seriesScatterRGB;
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
