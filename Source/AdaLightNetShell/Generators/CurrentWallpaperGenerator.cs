using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;
using SlimDX.Direct3D10;

namespace AdaLightNetShell.Generators
{
    public class CurrentWallpaperGenerator : ILedGenerator
    {
        private DateTime _lastWriteTime;
        public void Dispose()
        {

        }

        public void Initialize()
        {
            _lastWriteTime = DateTime.MinValue;
        }

        private int _counter;

        public bool Generate(byte[] ledArray)
        {
            if (Interlocked.Increment(ref _counter) != 1)
            {
                return false;
            }

            try
            {
                var wallPaperFilePath = GetCurrentWallPaperPath();
                if (File.Exists(wallPaperFilePath))
                {
                    var lastWriteTime = File.GetLastWriteTime(wallPaperFilePath);
                    if (lastWriteTime == _lastWriteTime)
                    {
                        return true; // nothing has changed
                    }

                    byte[] wallpaperRgbArray;
                    try
                    {
                        int wallpaperWidth, wallpaperHeight;
                        using (var image = Image.FromFile(wallPaperFilePath))
                        {
                            wallpaperWidth = image.Width;
                            wallpaperHeight = image.Height;
                            using (var bitmap = new Bitmap(image))
                            {
                                wallpaperRgbArray = GetRGB(bitmap);
                            }
                        }

                        _lastWriteTime = lastWriteTime;

                        System.Diagnostics.Debug.WriteLine("Changed" + wallpaperRgbArray.Length);

                        int heightOffset = 0;
                        int boxHeight = wallpaperHeight / 6;
                        int boxWidth = wallpaperWidth / 9;
                        int widthOffset = wallpaperWidth * 3;
                        var averages = new int[LedConstants.LED_ARRAY_SIZE];
                        var pixelsInBox = boxHeight * boxWidth;
                        var boxOffset = boxWidth * 3;
                        var emptyAreaOffset = widthOffset - 2 * boxOffset;

                        // TOP screen
                        for (int h = 0; h < boxHeight; ++h)
                        {
                            int avgPos = 8 * LedConstants.BYTES_PER_LED;

                            int p = heightOffset;

                            for (int boxId = 8; boxId < 17; ++boxId)
                            {
                                for (int w = 0; w < boxWidth; ++w)
                                {
                                    averages[avgPos + 2] += wallpaperRgbArray[p];
                                    averages[avgPos + 1] += wallpaperRgbArray[++p];
                                    averages[avgPos] += wallpaperRgbArray[++p];
                                    ++p;
                                }
                                avgPos += LedConstants.BYTES_PER_LED; // skip to next box
                            }

                            heightOffset += widthOffset; // skip to next line
                        }

                        int leftBoxId = 7;
                        int rightBoxId = 17;

                        for (int y = 0; y < 4; y++)
                        {
                            for (int h = 0; h < boxHeight; ++h)
                            {
                                int p = heightOffset;

                                int avgPos = leftBoxId * LedConstants.BYTES_PER_LED;
                                for (int w = 0; w < boxWidth; ++w)
                                {
                                    averages[avgPos + 2] += wallpaperRgbArray[p];
                                    averages[avgPos + 1] += wallpaperRgbArray[++p];
                                    averages[avgPos] += wallpaperRgbArray[++p];
                                    ++p;
                                }

                                p += emptyAreaOffset;

                                avgPos = rightBoxId * LedConstants.BYTES_PER_LED;
                                for (int w = 0; w < boxWidth; ++w)
                                {
                                    averages[avgPos + 2] += wallpaperRgbArray[p];
                                    averages[avgPos + 1] += wallpaperRgbArray[++p];
                                    averages[avgPos] += wallpaperRgbArray[++p];
                                    ++p;
                                }

                                heightOffset += widthOffset; // skip to next line
                            }

                            --leftBoxId;
                            ++rightBoxId;
                        }

                        // BOTTOM:
                        for (int h = 0; h < boxHeight; ++h)
                        {
                            int p = heightOffset;
                            for (int boxId = 3; boxId >= 0; --boxId)
                            {
                                int avgPos = boxId * 3;

                                for (int w = 0; w < boxWidth; ++w)
                                {
                                    averages[avgPos + 2] += wallpaperRgbArray[p];
                                    averages[avgPos + 1] += wallpaperRgbArray[++p];
                                    averages[avgPos] += wallpaperRgbArray[++p];
                                    ++p;
                                }
                            }

                            p += boxOffset; // skip empty box

                            for (int boxId = 24; boxId >= 21; --boxId)
                            {
                                int avgPos = boxId * LedConstants.BYTES_PER_LED;

                                for (int w = 0; w < boxWidth; ++w)
                                {
                                    averages[avgPos + 2] += wallpaperRgbArray[p];
                                    averages[avgPos + 1] += wallpaperRgbArray[++p];
                                    averages[avgPos] += wallpaperRgbArray[++p];
                                    ++p;
                                }
                            }

                            heightOffset += widthOffset; // skip to next line
                        }

                        {
                            int avgPos = 0;
                            for (int boxId = 0; boxId < 25; boxId++)
                            {
                                // calculate average color
                                ledArray[avgPos] = (byte)(averages[avgPos++] / pixelsInBox);
                                ledArray[avgPos] = (byte)(averages[avgPos++] / pixelsInBox);
                                ledArray[avgPos] = (byte)(averages[avgPos++] / pixelsInBox);
                            }
                        }
                    }
                    catch (OutOfMemoryException)
                    {

                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref _counter, 0);
            }
            return true;
        }

        private string GetCurrentWallPaperPath()
        {
            using (var regKey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", false))
            {

                if (regKey != null)
                {
                    return regKey.GetValue("WallPaper").ToString();
                }
            }

            return null;
        }

        private byte[] GetRGB(Bitmap image)
        {
            var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            try
            {
                var pixelData = new byte[data.Stride * data.Height];
                Marshal.Copy(data.Scan0, pixelData, 0, data.Stride * data.Height);

                return pixelData;
            }
            finally
            {
                image.UnlockBits(data);
            }
        }
    }
}