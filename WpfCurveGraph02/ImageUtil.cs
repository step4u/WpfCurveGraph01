using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfCurveGraph02
{
    internal class ImageUtil
    {
        internal static void InverseImage(WriteableBitmap bitmap)
        {
            bitmap.Lock();

            // 픽셀 데이터 가져오기
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = bitmap.BackBufferStride;
            IntPtr buffer = bitmap.BackBuffer;

            unsafe
            {
                byte* pPixels = (byte*)buffer.ToPointer();

                // 각 픽셀의 색상 값 반전 (BGR 포맷으로 가정)
                for (int y = 0; y < height; y++)
                {
                    byte* row = pPixels + (y * stride);

                    for (int x = 0; x < width; x++)
                    {
                        row[x * 4] = (byte)(255 - row[x * 4]);         // Blue
                        row[x * 4 + 1] = (byte)(255 - row[x * 4 + 1]); // Green
                        row[x * 4 + 2] = (byte)(255 - row[x * 4 + 2]); // Red
                                                                       // row[x * 4 + 3]는 알파 값 (투명도), 변경하지 않음
                    }
                }
            }

            bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            bitmap.Unlock();
        }

        internal static void ApplyFilter2Image(WriteableBitmap bitmap, byte[] lut, int channel = -1)
        {
            bitmap.Lock();

            try
            {
                // 픽셀 데이터 가져오기
                int width = bitmap.PixelWidth;
                int height = bitmap.PixelHeight;
                int stride = bitmap.BackBufferStride;
                IntPtr buffer = bitmap.BackBuffer;

                unsafe
                {
                    byte* pPixels = (byte*)buffer.ToPointer();

                    for (int y = 0; y < bitmap.PixelHeight; y++)
                    {
                        for (int x = 0; x < bitmap.PixelWidth; x++)
                        {
                            // 현재 픽셀 위치 계산
                            int index = y * stride + x * 4;

                            // BGR 채널에 LUT 적용
                            if (channel == 2)
                            {
                                byte red = lut[pPixels[index + 2]];
                                pPixels[index + 2] = red;
                            }
                            else if (channel == 1)
                            {
                                byte green = lut[pPixels[index + 1]];
                                pPixels[index + 1] = green;
                            }
                            else if (channel == 0)
                            {
                                byte blue = lut[pPixels[index]];
                                pPixels[index] = blue;
                            }
                            else
                            {
                                byte blue = lut[pPixels[index]];                // Blue 채널
                                byte green = lut[pPixels[index + 1]];           // Green 채널
                                byte red = lut[pPixels[index + 2]];             // Red 채널

                                // BackBuffer에 새로운 값 할당
                                pPixels[index] = blue;
                                pPixels[index + 1] = green;
                                pPixels[index + 2] = red;
                            }
                        }
                    }
                }

                bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            }
            catch (Exception ex)
            {
            }
            finally
            {
                bitmap.Unlock();
            }
        }

        internal static void ApplyFilter2Image2(WriteableBitmap oBitmap, WriteableBitmap bitmap, byte[] lut, int channel = -1)
        {
            oBitmap.CopyPixels(new Int32Rect(0, 0, oBitmap.PixelWidth, oBitmap.PixelHeight),
                                      bitmap.BackBuffer, bitmap.BackBufferStride * bitmap.PixelHeight,
                                      bitmap.BackBufferStride);

            bitmap.Lock();

            try
            {
                // 픽셀 데이터 가져오기
                int width = bitmap.PixelWidth;
                int height = bitmap.PixelHeight;
                int stride = bitmap.BackBufferStride;
                IntPtr buffer = bitmap.BackBuffer;

                unsafe
                {
                    byte* pPixels = (byte*)buffer.ToPointer();

                    for (int y = 0; y < bitmap.PixelHeight; y++)
                    {
                        for (int x = 0; x < bitmap.PixelWidth; x++)
                        {
                            // 현재 픽셀 위치 계산
                            int index = y * stride + x * 4;

                            // BGR 채널에 LUT 적용
                            if (channel == 2)
                            {
                                byte red = lut[pPixels[index + 2]];
                                pPixels[index + 2] = red;
                            }
                            else if (channel == 1)
                            {
                                byte green = lut[pPixels[index + 1]];
                                pPixels[index + 1] = green;
                            }
                            else if (channel == 0)
                            {
                                byte blue = lut[pPixels[index]];
                                pPixels[index] = blue;
                            }
                            else
                            {
                                byte blue = lut[pPixels[index]];                // Blue 채널
                                byte green = lut[pPixels[index + 1]];           // Green 채널
                                byte red = lut[pPixels[index + 2]];             // Red 채널

                                // BackBuffer에 새로운 값 할당
                                pPixels[index] = blue;
                                pPixels[index + 1] = green;
                                pPixels[index + 2] = red;
                            }
                        }
                    }
                }

                bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            }
            catch (Exception ex)
            {
            }
            finally
            {
                bitmap.Unlock();
            }
        }

        internal static void ApplyFilter2Image3(WriteableBitmap oBitmap, WriteableBitmap bitmap, byte[] lut, int channel = -1)
        {
            oBitmap.CopyPixels(new Int32Rect(0, 0, oBitmap.PixelWidth, oBitmap.PixelHeight),
                                      bitmap.BackBuffer, bitmap.BackBufferStride * bitmap.PixelHeight,
                                      bitmap.BackBufferStride);

            bitmap.Lock();

            try
            {
                int width = bitmap.PixelWidth;
                int height = bitmap.PixelHeight;
                int stride = bitmap.BackBufferStride;
                int length = height * stride;

                unsafe
                {
                    // BackBuffer에 직접 접근
                    byte* buffer = (byte*)bitmap.BackBuffer;

                    // 픽셀 데이터 순회 (BGRA 포맷)
                    for (int i = 0; i < length; i += 4)
                    {
                        buffer[i] = lut[buffer[i]];       // B 채널
                        buffer[i + 1] = lut[buffer[i + 1]]; // G 채널
                        buffer[i + 2] = lut[buffer[i + 2]]; // R 채널
                                                            // buffer[i + 3]는 알파 채널로 건너뜀 (필요한 경우 처리 가능)
                    }
                }

                bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            }
            catch (Exception ex)
            {
            }
            finally
            {
                bitmap.Unlock();
            }
        }

        internal static void ResetImage(WriteableBitmap bitmap)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            byte[] clearData = new byte[width * height * 4];
            Int32Rect fullRect = new Int32Rect(0, 0, width, height);

            bitmap.WritePixels(fullRect, clearData, width * 4, 0);
        }

    }
}
