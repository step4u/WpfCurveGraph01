using MathNet.Numerics.Interpolation;
using MathNet.Numerics.LinearAlgebra.Factorization;
using SkiaSharp;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.XPath;
using Telerik.Charting;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.DataVisualization.Map.BingRest;
using Telerik.Windows.Controls.Map;
using Point = System.Windows.Point;

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

            //CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        Stopwatch _stopwatch = new Stopwatch();
        int _frameCounter = 0;
        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            if (_frameCounter++ == 0)
                _stopwatch.Start();

            // Determine frame rate in fps (frames per second).
            long frameRate = (long)(_frameCounter / this._stopwatch.Elapsed.TotalSeconds);

            Debug.WriteLine($"CompositionTarget_Rendering->frameRate: {frameRate} ");
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox control = (ComboBox)sender;

            if (!control.IsMouseOver)
                return;

            vmodel.SelectionChangedCurveCombo(sender, e);
        }

        bool HasNearPoint = false;
        Point? prevScatter = null;
        Point? nextScatter = null;
        Point? selectedScatter = null;
        Point? closestPointCurve = null;


        SKBitmap? bitmap = null;
        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, 0, 0);
        BitmapData? srcData = null;
        int width = 0;
        int height = 0;

        private void RadCartesianChart_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            var chart = sender as RadCartesianChart;
            if (chart == null) return;

            var channel = vmodel.SelectedItemHistoCombo;
            var curveData = vmodel.SeriesCurve;
            var scatterData = vmodel.SeriesScatter;

            //using (var stream = new SKMemoryStream(vmodel.SelectedImageBuffer))
            //{
            //    bitmap = SKBitmap.Decode(stream);
            //}

            //width = bitmap.Width;
            //height = bitmap.Height;
           

            if (curveData != null)
            {
                var chartPos = e.GetPosition(chart);
                var pos = new Point(
                    (int)(Math.Min(1, Math.Max(0, (chartPos.X / chart.ActualWidth))) * 255),
                    (int)(Math.Min(1, Math.Max(0, ((chart.ActualHeight - chartPos.Y) / chart.ActualHeight))) * 255));

                finder.PointCollections = scatterData;
                selectedScatter = finder.FindClosestPoint(pos, detectDistance);

                if (selectedScatter == null)
                {
                    finder.PointCollections = curveData;
                    closestPointCurve = finder.FindClosestPoint(pos, detectDistance);

                    if (closestPointCurve != null)
                    {
                        selectedScatter = closestPointCurve;

                        var idx = FindInsertIndex(scatterData, (Point)selectedScatter);

                        scatterData.Insert(idx, (Point)selectedScatter);

                        prevScatter = scatterData.LastOrDefault(x => x.X < selectedScatter.Value.X);
                        if (prevScatter == null)
                            prevScatter = new Point(0, 0);

                        nextScatter = scatterData.FirstOrDefault(x => x.X > selectedScatter.Value.X);
                        if (nextScatter == null)
                            nextScatter = new Point(255, 255);
                    }
                }
                else
                {
                    prevScatter = scatterData.FirstOrDefault(x => x.X < selectedScatter.Value.X);
                    if (prevScatter == null)
                        prevScatter = new Point(0,0);

                    nextScatter = scatterData.FirstOrDefault(x => x.X > selectedScatter.Value.X);
                    if (nextScatter == null)
                        nextScatter = new Point(255, 255);
                }

                curveIsDragging = true;
                chart.CaptureMouse();
            }
        }

        int FindInsertIndex(ObservableCollection<Point> list, Point newPoint)
        {
            for (int i = 0; i < list.Count; i++)
            {
                // Compare based on X coordinate; use Y as a tiebreaker if needed.
                if (newPoint.X < list[i].X ||
                    (newPoint.X == list[i].X && newPoint.Y < list[i].Y))
                {
                    return i;
                }
            }
            // If no smaller point is found, return the index to add at the end.
            return list.Count;
        }

        int detectDistance = 5;
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

                var idx = scatterData.IndexOf(selectedScatter ?? new Point(-1,-1));
                if (idx > -1)
                {
                    if (curPoint.X < prevScatter!.Value.X + detectDistance)
                        curPoint.X = prevScatter!.Value.X + detectDistance;

                    if (curPoint.X < nextScatter!.Value.X && curPoint.X > nextScatter!.Value.X + detectDistance)
                        curPoint.X = nextScatter!.Value.X - detectDistance;

                    if (curPoint.X < 0)
                        curPoint.X = 0;
                    else if (curPoint.X > 255)
                        curPoint.X = 255;

                    if (curPoint.Y < 0)
                        curPoint.Y = 0;
                    else if (curPoint.Y > 255)
                        curPoint.Y = 255;

                    scatterData[idx] = curPoint;
                    selectedScatter = curPoint;

                    //Tuple<ObservableCollection<Point>, ObservableCollection<Point>> tuple = Tuple.Create(scatterData, curveData);
                    //Thread t = new Thread(new ParameterizedThreadStart(UpdateCurve));
                    //t.Start((tuple.Item1, tuple.Item2));

                    UpdateCurve(scatterData, curveData);
                }
            }
            else
            {
                bool isNearScatterPoint = scatterData.Any(point => Math.Abs(point.X - curPoint.X) <= detectDistance && Math.Abs(point.Y - curPoint.Y) <= detectDistance);

                if (isNearScatterPoint)
                {
                    Mouse.OverrideCursor = Cursors.ScrollAll;

                    HasNearPoint = true;
                }
                else
                {
                    bool isNearPoint = curveData.Any(point => Math.Abs(point.X - curPoint.X) <= detectDistance && Math.Abs(point.Y - curPoint.Y) <= detectDistance);

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

            bitmap = null;
        }

        private void btnGetPoints_Click(object sender, RoutedEventArgs e)
        {
            //var scatterData = vmodel.SeriesScatter;

            //List<Point> interpolatedPoints = GetSplinePoints(scatterData);
            //foreach (var point in interpolatedPoints)
            //{
            //    Console.WriteLine($"X: {point.X}, Y: {point.Y}");
            //}
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
            //Dispatcher.Invoke(DispatcherPriority.Normal, new Action(async delegate
            //{

            //}));

            double[] xValues = scatters!.Select(p => (double)p.X).ToArray();
            double[] yValues = scatters!.Select(p => (double)p.Y).ToArray();

            var spline = CubicSpline.InterpolateNatural(xValues, yValues);

            //var integerPoints = new ObservableCollection<Point>();
            byte[] lut = new byte[256];

            for (int x = 0; x <= 255; x++)
            {
                double y = spline.Interpolate(x);
                y = Math.Clamp(y, 0, 255);

                //integerPoints.Add(new Point(x, (int)y));

                curves![x] = new Point(x, (int)y);

                lut[x] = (byte)(int)y;
            }

            //Tuple<Bitmap, byte[]> tuple = Tuple.Create(new Bitmap(new MemoryStream(vmodel.SelectedImageBuffer!)), lut);
            //Thread t = new Thread(new ParameterizedThreadStart(ApplyCurveFilter));
            //t.IsBackground = true;
            //t.Start((tuple.Item1, tuple.Item2));


            ////using var inputImage = SKBitmap.Decode(SKData.CreateCopy(vmodel.SelectedImageBuffer));
            //Task.Run(() => ApplyCurveFilterAsync(lut));

            ////// 5. 조정된 이미지 저장
            ////using var image = SKImage.FromBitmap(adjustedImage);
            //curveLayer.Source = ToBitmapSource(adjustedImage);

            //return lut;
        }

        private Bitmap CreateSaturationLayer(int width, int height, double saturation)
        {
            var layer = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(layer))
            {
                // 투명도를 포함한 흰색으로 초기화
                ColorMatrix colorMatrix = new ColorMatrix
                (
                    new float[][]
                    {
                        new float[] { 1, 0, 0, 0, 0 },
                        new float[] { 0, 1, 0, 0, 0 },
                        new float[] { 0, 0, 1, 0, 0 },
                        new float[] { 0, 0, 0, 0.5f, 0 },  // 알파(투명도) 50%
                        new float[] { 0, 0, 0, 0, 1 }
                    }
                );

                var imageAttributes = new System.Drawing.Imaging.ImageAttributes();
                imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                // 레이어의 색상 조정
                g.FillRectangle(new SolidBrush(System.Drawing.Color.White), 0, 0, width, height);
            }

            return layer;
        }

        private async Task ApplyCurveFilterAsync(byte[] lut)
        {
            try
            {
                //var adjustedImage = await Task.Run(() => ApplyCurveFilter0(lut));
                var adjustedImage = await Task.Run(() => ApplyCurveFilter1(lut));

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    img.Source = SKBitmapToBitmapImage(adjustedImage!);
                }));
            }
            catch (Exception ex)
            {
            }
        }

        //private Bitmap? ApplyCurveFilter0(byte[] lut)
        //{
        //    Debug.WriteLine("ApplyCurveFilter0 >>> ");

        //    //var width = bitmap!.Width;
        //    //var height = bitmap!.Height;

        //    Bitmap? result = null;

        //    try
        //    {
        //        result = new Bitmap(width, height, bitmap!.PixelFormat);

        //        // 비트맵 데이터 락킹으로 빠르게 처리
        //        //var rect = new System.Drawing.Rectangle(0, 0, width, height);
        //        //var srcData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
        //        var dstData = result.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, result.PixelFormat);

        //        int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
        //        int byteCount = dstData.Stride * height;
        //        byte[] pixelBuffer = new byte[byteCount];

        //        Marshal.Copy(srcData!.Scan0, pixelBuffer, 0, byteCount);
        //        //bitmap.UnlockBits(srcData);

        //        // LUT를 사용해 각 픽셀에 필터를 적용
        //        for (int i = 0; i < pixelBuffer.Length; i += bytesPerPixel)
        //        {
        //            pixelBuffer[i] = lut[pixelBuffer[i]];      // Blue 채널
        //            pixelBuffer[i + 1] = lut[pixelBuffer[i + 1]];  // Green 채널
        //            pixelBuffer[i + 2] = lut[pixelBuffer[i + 2]];  // Red 채널
        //                                                           // 알파 값은 그대로 유지 (ARGB 형식인 경우)
        //        }

        //        Marshal.Copy(pixelBuffer, 0, dstData.Scan0, byteCount);
        //        result.UnlockBits(dstData);
        //    }
        //    catch (Exception ex)
        //    {
        //        result = null;
        //    }

        //    return result;
        //}

        SKBitmap? ApplyCurveFilter1(byte[] lut)
        {
            try
            {
                var adjusted = new SKBitmap(bitmap!.Width, bitmap!.Height);

                using (var srcPixmap = bitmap!.PeekPixels())
                using (var dstPixmap = adjusted.PeekPixels())
                {
                    IntPtr srcPtr = srcPixmap.GetPixels();
                    IntPtr dstPtr = dstPixmap.GetPixels();

                    unsafe
                    {
                        byte* src = (byte*)srcPtr.ToPointer();
                        byte* dst = (byte*)dstPtr.ToPointer();
                        int length = width * height * 4; // RGBA = 4 bytes per pixel

                        // 병렬 처리: 이미지 크기에 따라 작업을 나눔
                        Parallel.For(0, length / 4, i =>
                        {
                            int index = i * 4;
                            dst[index] = lut[src[index]];       // Blue
                            dst[index + 1] = lut[src[index + 1]]; // Green
                            dst[index + 2] = lut[src[index + 2]]; // Red
                            dst[index + 3] = src[index + 3];     // Alpha 그대로 복사
                        });
                    }
                }

                return adjusted;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void ApplyCurveFilter(object? obj)
        {
            if (obj is ValueTuple<Bitmap, byte[]> tuple)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    var width = tuple.Item1.Width;
                    var height = tuple.Item1.Height;

                    var result = new Bitmap(width, height, tuple.Item1.PixelFormat);

                    // 비트맵 데이터 락킹으로 빠르게 처리
                    var rect = new System.Drawing.Rectangle(0, 0, width, height);
                    var srcData = tuple.Item1.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, tuple.Item1.PixelFormat);
                    var dstData = result.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, result.PixelFormat);

                    int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(tuple.Item1.PixelFormat) / 8;
                    int byteCount = dstData.Stride * height;
                    byte[] pixelBuffer = new byte[byteCount];

                    Marshal.Copy(srcData.Scan0, pixelBuffer, 0, byteCount);
                    tuple.Item1.UnlockBits(srcData);

                    // LUT를 사용해 각 픽셀에 필터를 적용
                    for (int i = 0; i < pixelBuffer.Length; i += bytesPerPixel)
                    {
                        pixelBuffer[i] = tuple.Item2[pixelBuffer[i]];      // Blue 채널
                        pixelBuffer[i + 1] = tuple.Item2[pixelBuffer[i + 1]];  // Green 채널
                        pixelBuffer[i + 2] = tuple.Item2[pixelBuffer[i + 2]];  // Red 채널
                                                                               // 알파 값은 그대로 유지 (ARGB 형식인 경우)
                    }

                    Marshal.Copy(pixelBuffer, 0, dstData.Scan0, byteCount);
                    result.UnlockBits(dstData);

                    img.Source = ToBitmapSource(result);
                }));

                //return result;
            }
        }


        //public static Bitmap ApplyCurveFilter(Bitmap original, byte[] lut)
        //{
        //    // Clone the original bitmap to avoid thread conflicts
        //    Bitmap adjusted = new Bitmap(original.Width, original.Height, original.PixelFormat);

        //    // Lock the original and adjusted bitmaps in memory
        //    System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, original.Width, original.Height);
        //    BitmapData srcData = original.LockBits(rect, ImageLockMode.ReadOnly, original.PixelFormat);
        //    BitmapData dstData = adjusted.LockBits(rect, ImageLockMode.WriteOnly, original.PixelFormat);

        //    int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(original.PixelFormat) / 8;
        //    int byteCount = srcData.Stride * original.Height;
        //    byte[] pixelBuffer = new byte[byteCount];

        //    // Copy the original bitmap's pixel data to a byte array
        //    Marshal.Copy(srcData.Scan0, pixelBuffer, 0, byteCount);
        //    original.UnlockBits(srcData); // Unlock the original bitmap

        //    // Process the pixel data using LUT with Parallel.For
        //    Parallel.For(0, original.Height, y =>
        //    {
        //        int yOffset = y * srcData.Stride;

        //        for (int x = 0; x < original.Width; x++)
        //        {
        //            int pixelIndex = yOffset + (x * bytesPerPixel);

        //            // Apply LUT transformation (BGR order)
        //            pixelBuffer[pixelIndex] = lut[pixelBuffer[pixelIndex]];       // Blue
        //            pixelBuffer[pixelIndex + 1] = lut[pixelBuffer[pixelIndex + 1]]; // Green
        //            pixelBuffer[pixelIndex + 2] = lut[pixelBuffer[pixelIndex + 2]]; // Red
        //                                                                            // Alpha remains unchanged (if present)
        //        }
        //    });

        //    // Copy the modified pixel data back to the adjusted bitmap
        //    Marshal.Copy(pixelBuffer, 0, dstData.Scan0, byteCount);
        //    adjusted.UnlockBits(dstData); // Unlock the adjusted bitmap

        //    return adjusted;
        //}



        //SKBitmap ApplyCurveFilter(SKBitmap original, byte[] lut)
        //{
        //    // 원본과 동일한 크기의 Bitmap 생성
        //    var adjusted = new SKBitmap(original.Width, original.Height);

        //    // 각 픽셀에 LUT 적용
        //    for (int y = 0; y < original.Height; y++)
        //    {
        //        for (int x = 0; x < original.Width; x++)
        //        {
        //            SKColor pixel = original.GetPixel(x, y);

        //            // LUT를 사용해 RGB 값 변환
        //            byte r = lut[pixel.Red];
        //            byte g = lut[pixel.Green];
        //            byte b = lut[pixel.Blue];

        //            // 새로운 색상 설정
        //            adjusted.SetPixel(x, y, new SKColor(r, g, b, pixel.Alpha));
        //        }
        //    }

        //    return adjusted;
        //}

        //SKBitmap ApplyCurveFilter(SKBitmap original, byte[] lut)
        //{
        //    var adjusted = new SKBitmap(original.Width, original.Height);

        //    using (var srcPixmap = original.PeekPixels())
        //    using (var dstPixmap = adjusted.PeekPixels())
        //    {
        //        IntPtr srcPtr = srcPixmap.GetPixels();
        //        IntPtr dstPtr = dstPixmap.GetPixels();

        //        unsafe
        //        {
        //            byte* src = (byte*)srcPtr.ToPointer();
        //            byte* dst = (byte*)dstPtr.ToPointer();
        //            int length = original.Width * original.Height * 4; // RGBA = 4 bytes per pixel

        //            // 병렬 처리: 이미지 크기에 따라 작업을 나눔
        //            Parallel.For(0, length / 4, i =>
        //            {
        //                int index = i * 4;
        //                dst[index] = lut[src[index]];       // Blue
        //                dst[index + 1] = lut[src[index + 1]]; // Green
        //                dst[index + 2] = lut[src[index + 2]]; // Red
        //                dst[index + 3] = src[index + 3];     // Alpha 그대로 복사
        //            });
        //        }
        //    }

        //    return adjusted;
        //}

        public BitmapImage? SKBitmapToBitmapImage(SKBitmap skBitmap)
        {
            try
            {
                if (skBitmap == null)
                    throw new ArgumentNullException(nameof(skBitmap));

                // Step 1: Encode SKBitmap to a byte array (PNG format)
                using (var image = SKImage.FromBitmap(skBitmap))
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                {
                    return ByteArrayToBitmapImage(data.ToArray());
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private BitmapImage ByteArrayToBitmapImage(byte[] imageData)
        {
            var bitmapImage = new BitmapImage();
            using (var stream = new MemoryStream(imageData))
            {
                stream.Position = 0; // Reset stream position

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // Ensure image is loaded into memory
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Freeze for thread safety
            }
            return bitmapImage;
        }

        private BitmapSource? ToBitmapSource(Bitmap bitmap)
        {
            var hBitmap = bitmap.GetHbitmap(); // 핸들을 가져옵니다.
            try
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                DeleteObject(hBitmap); // 리소스 해제
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using MemoryStream memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            memoryStream.Position = 0;

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        ImageSource ToImageSource(SKImage skImage)
        {
            using (var data = skImage.Encode(SKEncodedImageFormat.Png, 100)) // Encode as PNG
            using (var stream = new MemoryStream(data.ToArray()))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze(); // Freeze to use across threads

                return bitmap;
            }
        }

    }
}