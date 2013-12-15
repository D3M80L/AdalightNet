using System;
using System.Windows.Forms;
using AdaLightNetShell.Infrastructure;
using SlimDX.Direct3D9;
using System.Threading;

namespace AdaLightNetShell.Generators
{
    public sealed class SlimDxScreenGenerator : ILedGenerator
    {
        public const byte BYTES_PER_PIXEL = 4;

        private Device _device;
        private Surface _surface;

        private int _screenWidth;
        private int _screenHeight;

        /// <summary>
        /// How many bytes are hold for one line
        /// </summary>
        private int _widthOffset;
        private int _screenBufferSize;
        private byte[] _screenBuffer;

        private int _boxHeight;
        private int _boxWidth;
        private int _pixelsInBox;
        private int _emptyAreaOffset;
        private int _boxOffset;

        private int[] _averages;

        private int _counter = 0;
        private int _lockCounter = 0;
        public bool Generate(byte[] ledArray)
        {
            if (Interlocked.Increment(ref _counter) < 10)
            {
                return false;
            }

            if (Interlocked.Increment(ref _lockCounter) != 1)
            {
                return false;
            }
            Interlocked.Exchange(ref _counter, 0);

            CaptureScreen();

            int heightOffset = 0;

            // TOP screen
            for (int h = 0; h < _boxHeight; ++h)
            {
                int avgPos = 8 * LedConstants.BYTES_PER_LED;

                int p = heightOffset;

                for (int boxId = 8; boxId < 17; ++boxId)
                {
                    for (int w = 0; w < _boxWidth; ++w)
                    {
                        _averages[avgPos + 2] += _screenBuffer[p];
                        _averages[avgPos + 1] += _screenBuffer[++p];
                        _averages[avgPos] += _screenBuffer[++p];
                        p += 2;
                    }
                    avgPos += LedConstants.BYTES_PER_LED; // skip to next box
                }

                heightOffset += _widthOffset; // skip to next line
            }

            // MIDDLE: left and right
            int leftBoxId = 7;
            int rightBoxId = 17;

            for (int y = 0; y < 4; y++)
            {
                for (int h = 0; h < _boxHeight; ++h)
                {
                    int p = heightOffset;

                    int avgPos = leftBoxId * LedConstants.BYTES_PER_LED;
                    for (int w = 0; w < _boxWidth; ++w)
                    {
                        _averages[avgPos + 2] += _screenBuffer[p];
                        _averages[avgPos + 1] += _screenBuffer[++p];
                        _averages[avgPos] += _screenBuffer[++p];
                        p += 2;
                    }

                    p += _emptyAreaOffset;

                    avgPos = rightBoxId * LedConstants.BYTES_PER_LED;
                    for (int w = 0; w < _boxWidth; ++w)
                    {
                        _averages[avgPos + 2] += _screenBuffer[p];
                        _averages[avgPos + 1] += _screenBuffer[++p];
                        _averages[avgPos] += _screenBuffer[++p];
                        p += 2;
                    }

                    heightOffset += _widthOffset; // skip to next line
                }

                --leftBoxId;
                ++rightBoxId;
            }

            // BOTTOM:
            for (int h = 0; h < _boxHeight; ++h)
            {
                int p = heightOffset;
                for (int boxId = 3; boxId >= 0; --boxId)
                {
                    int avgPos = boxId * 3;

                    for (int w = 0; w < _boxWidth; ++w)
                    {
                        _averages[avgPos + 2] += _screenBuffer[p];
                        _averages[avgPos + 1] += _screenBuffer[++p];
                        _averages[avgPos] += _screenBuffer[++p];
                        p += 2;
                    }
                }

                p += _boxOffset; // skip empty box

                for (int boxId = 24; boxId >= 21; --boxId)
                {
                    int avgPos = boxId * LedConstants.BYTES_PER_LED;

                    for (int w = 0; w < _boxWidth; ++w)
                    {
                        _averages[avgPos + 2] += _screenBuffer[p];
                        _averages[avgPos + 1] += _screenBuffer[++p];
                        _averages[avgPos] += _screenBuffer[++p];
                        p += 2;
                    }
                }

                heightOffset += _widthOffset; // skip to next line
            }

            {
                int avgPos = 0;
                for (int boxId = 0; boxId < 25; boxId++)
                {
                    // calculate average color
                    ledArray[avgPos] = (byte)(_averages[avgPos] / _pixelsInBox);
                    ledArray[avgPos + 1] = (byte)(_averages[avgPos + 1] / _pixelsInBox);
                    ledArray[avgPos + 2] = (byte)(_averages[avgPos + 2] / _pixelsInBox);

                    // clean averages for next use
                    _averages[avgPos] = 0;
                    _averages[++avgPos] = 0;
                    _averages[++avgPos] = 0;
                    ++avgPos;
                }
            }

            Interlocked.Exchange(ref _lockCounter, 0);
            return true;
        }

        public void Dispose()
        {
            try
            {
                _surface.Dispose();
                _device.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            finally
            {
                _surface = null;
                _device = null;
                _averages = null;
            }
        }

        private void CaptureScreen()
        {
            _device.GetFrontBufferData(0, _surface);

            var dr = _surface.LockRectangle(LockFlags.None);

            var gs = dr.Data;

            gs.Position = 0;
            gs.Read(_screenBuffer, 0, _screenBufferSize);
            _surface.UnlockRectangle();
        }

        public void Initialize()
        {
            InternalInitialize();
            Log.Info("Capturing screen.");
        }
        
        private void InternalInitialize()
        {
            _screenWidth = Screen.PrimaryScreen.Bounds.Width;
            _screenHeight = Screen.PrimaryScreen.Bounds.Height;

            _screenBufferSize = _screenWidth * _screenHeight * BYTES_PER_PIXEL;
            _screenBuffer = new byte[_screenBufferSize];
            _widthOffset = _screenWidth * BYTES_PER_PIXEL;
            

            _boxHeight = _screenHeight / 6;
            _boxWidth = _screenWidth / 9;
            _pixelsInBox = _boxWidth * _boxHeight;

            _boxOffset = _boxWidth * BYTES_PER_PIXEL;
            _emptyAreaOffset = _widthOffset - 2 * _boxOffset;

            _averages = new int[LedConstants.LED_ARRAY_SIZE];

            var parameters = new PresentParameters();
            parameters.Windowed = true;
            parameters.SwapEffect = SwapEffect.Discard;
            _device = new Device(new Direct3D(), 0, DeviceType.Hardware, IntPtr.Zero, CreateFlags.SoftwareVertexProcessing, parameters);
            _surface = Surface.CreateOffscreenPlain(_device, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, Format.A8R8G8B8, Pool.Scratch); 
        }
    }
}
