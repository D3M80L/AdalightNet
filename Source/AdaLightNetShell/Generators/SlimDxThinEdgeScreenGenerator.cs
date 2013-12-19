using System;
using System.Windows.Forms;
using AdaLightNetShell.Infrastructure;
using SlimDX.Direct3D9;
using System.Threading;

namespace AdaLightNetShell.Generators
{
    /// <summary>
    /// Each led are has different dimensions. There are two groups: top-bottom and left-right.
    /// This algorithm is faster than <see cref="SlimDxScreenGenerator"/>.
    /// </summary>
    /// <example>
    /// Let's assume that the screen resolution is 1920x1080. There are 9 leds at top and 6 on left.
    /// There is a constant which defines the border size. 
    /// MAX=32
    /// The size of a box in top and bottom area is calculated in this way:
    /// HeightA=Min(1080/6=180, MAX)
    /// WidthA = 1920/9 = 213
    /// The size of a box in left and right area is calculated in this way:
    /// WidthB=Min(1920/9=213,MAX)
    /// HeightB=(1080-2*HeightA)/4
    /// </example>
    public sealed class SlimDxThinEdgeScreenGenerator : ILedGenerator
    {
        public const byte BYTES_PER_PIXEL = 4;

        private Device _device;
        private Surface _surface;

        private int _screenWidth;
        private int _screenHeight;

        private int _screenBufferSize;
        private byte[] _screenBuffer;
        private int _widthOffset;

        private int[] _averages;

        private const int EDGE_SIZE = 32;

        /// <summary>
        /// Max allowed height of top and bottom border
        /// </summary>
        private const int MAX_HORIZONTAL_BOX_HEIGHT = EDGE_SIZE;
        /// <summary>
        /// Width of a box in TOP and BOTTOM
        /// </summary>
        private int _horizontalBoxWidth;
        /// <summary>
        /// Width of top and bottom BOXes
        /// </summary>
        private int _horizontalBoxHeight;
        private int _pixelsInHorizontalBox;
        private int _horizontalBoxOffset;

        /// <summary>
        /// Max allowed width of left and right border
        /// </summary>
        private const int MAX_VERTICAL_BOX_WIDTH = EDGE_SIZE;
        private int _verticalBoxWidth;
        private int _verticalBoxHeight;
        private int _pixelsInVerticalBox;

        private int _emptyAreaOffset;

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

            Metrics.Start();
            CaptureScreen();
            Metrics.Stop("Capture screen");

            Metrics.Start();
            int heightOffset = 0;

            // TOP screen
            for (int h = 0; h < _horizontalBoxHeight; ++h)
            {
                int avgPos = 8 * LedConstants.BYTES_PER_LED;

                int p = heightOffset;

                for (int boxId = 8; boxId < 17; ++boxId)
                {
                    for (int w = 0; w < _horizontalBoxWidth; ++w)
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
                for (int h = 0; h < _verticalBoxHeight; ++h)
                {
                    int p = heightOffset;

                    int avgPos = leftBoxId * LedConstants.BYTES_PER_LED;
                    for (int w = 0; w < _verticalBoxWidth; ++w)
                    {
                        _averages[avgPos + 2] += _screenBuffer[p];
                        _averages[avgPos + 1] += _screenBuffer[++p];
                        _averages[avgPos] += _screenBuffer[++p];
                        p += 2;
                    }

                    p += _emptyAreaOffset;

                    avgPos = rightBoxId * LedConstants.BYTES_PER_LED;
                    for (int w = 0; w < _verticalBoxWidth; ++w)
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
            for (int h = 0; h < _horizontalBoxHeight; ++h)
            {
                int p = heightOffset;
                for (int boxId = 3; boxId >= 0; --boxId)
                {
                    int avgPos = boxId * 3;

                    for (int w = 0; w < _horizontalBoxWidth; ++w)
                    {
                        _averages[avgPos + 2] += _screenBuffer[p];
                        _averages[avgPos + 1] += _screenBuffer[++p];
                        _averages[avgPos] += _screenBuffer[++p];
                        p += 2;
                    }
                }

                p += _horizontalBoxOffset; // skip empty box

                for (int boxId = 24; boxId >= 21; --boxId)
                {
                    int avgPos = boxId * LedConstants.BYTES_PER_LED;

                    for (int w = 0; w < _horizontalBoxWidth; ++w)
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

                for (int boxId = 0; boxId < 4; boxId++)
                {
                    // calculate average color
                    ledArray[avgPos] = (byte)(_averages[avgPos] / _pixelsInHorizontalBox);
                    ledArray[avgPos + 1] = (byte)(_averages[avgPos + 1] / _pixelsInHorizontalBox);
                    ledArray[avgPos + 2] = (byte)(_averages[avgPos + 2] / _pixelsInHorizontalBox);

                    // clean averages for next use
                    _averages[avgPos] = 0;
                    _averages[++avgPos] = 0;
                    _averages[++avgPos] = 0;
                    ++avgPos;
                }

                for (int boxId = 4; boxId < 8; boxId++)
                {
                    // calculate average color
                    ledArray[avgPos] = (byte)(_averages[avgPos] / _pixelsInVerticalBox);
                    ledArray[avgPos + 1] = (byte)(_averages[avgPos + 1] / _pixelsInVerticalBox);
                    ledArray[avgPos + 2] = (byte)(_averages[avgPos + 2] / _pixelsInVerticalBox);

                    // clean averages for next use
                    _averages[avgPos] = 0;
                    _averages[++avgPos] = 0;
                    _averages[++avgPos] = 0;
                    ++avgPos;
                }

                for (int boxId = 8; boxId < 17; boxId++)
                {
                    // calculate average color
                    ledArray[avgPos] = (byte)(_averages[avgPos] / _pixelsInHorizontalBox);
                    ledArray[avgPos + 1] = (byte)(_averages[avgPos + 1] / _pixelsInHorizontalBox);
                    ledArray[avgPos + 2] = (byte)(_averages[avgPos + 2] / _pixelsInHorizontalBox);

                    // clean averages for next use
                    _averages[avgPos] = 0;
                    _averages[++avgPos] = 0;
                    _averages[++avgPos] = 0;
                    ++avgPos;
                }

                for (int boxId = 17; boxId < 21; boxId++)
                {
                    // calculate average color
                    ledArray[avgPos] = (byte)(_averages[avgPos] / _pixelsInVerticalBox);
                    ledArray[avgPos + 1] = (byte)(_averages[avgPos + 1] / _pixelsInVerticalBox);
                    ledArray[avgPos + 2] = (byte)(_averages[avgPos + 2] / _pixelsInVerticalBox);

                    // clean averages for next use
                    _averages[avgPos] = 0;
                    _averages[++avgPos] = 0;
                    _averages[++avgPos] = 0;
                    ++avgPos;
                }

                for (int boxId = 21; boxId < 25; boxId++)
                {
                    // calculate average color
                    ledArray[avgPos] = (byte)(_averages[avgPos] / _pixelsInHorizontalBox);
                    ledArray[avgPos + 1] = (byte)(_averages[avgPos + 1] / _pixelsInHorizontalBox);
                    ledArray[avgPos + 2] = (byte)(_averages[avgPos + 2] / _pixelsInHorizontalBox);

                    // clean averages for next use
                    _averages[avgPos] = 0;
                    _averages[++avgPos] = 0;
                    _averages[++avgPos] = 0;
                    ++avgPos;
                }
            }

            Metrics.Stop("Calculations");
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

            _averages = new int[LedConstants.LED_ARRAY_SIZE];

            _horizontalBoxWidth = _screenWidth / 9;
            _horizontalBoxHeight = Math.Min(_screenHeight/9, MAX_HORIZONTAL_BOX_HEIGHT);
            _pixelsInHorizontalBox = _horizontalBoxWidth*_horizontalBoxHeight;
            _horizontalBoxOffset = _horizontalBoxWidth*BYTES_PER_PIXEL;

            _verticalBoxHeight = (_screenHeight - 2*_horizontalBoxHeight)/4;
            _verticalBoxWidth = Math.Min(_horizontalBoxWidth, MAX_VERTICAL_BOX_WIDTH);
            _pixelsInVerticalBox = _verticalBoxHeight*_verticalBoxWidth;
            _emptyAreaOffset = (_screenWidth - 2*_verticalBoxWidth)*BYTES_PER_PIXEL;

            var parameters = new PresentParameters();
            parameters.Windowed = true;
            parameters.SwapEffect = SwapEffect.Discard;
            _device = new Device(new Direct3D(), 0, DeviceType.Hardware, IntPtr.Zero, CreateFlags.SoftwareVertexProcessing, parameters);
            _surface = Surface.CreateOffscreenPlain(_device, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, Format.A8R8G8B8, Pool.Scratch);
        }
    }
}
