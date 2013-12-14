using System;

namespace AdaLightNetShell.Generators
{
    /// <summary>
    /// Rainbow generator taken from: http://learn.adafruit.com/adalight-diy-ambient-tv-lighting/running-the-software
    /// </summary>
    public class RainbowGenerator : ILedGenerator
    {
        public void Dispose()
        {

        }

        public void Initialize()
        {
            _sine = 0;
            _hue = 0;
        }

        private int _hue;
        private float _sine;

        public bool Generate(byte[] ledArray)
        {
            float sine2 = _sine;
            int hue2 = _hue, lo, r, g, b;

            for (int i = 0; i < LedConstants.LED_ARRAY_SIZE; )
            {
                lo = hue2 & 255;
                switch ((hue2 >> 8) % 6)
                {
                    case 0:
                        r = 255;
                        g = lo;
                        b = 0;
                        break;
                    case 1:
                        r = 255 - lo;
                        g = 255;
                        b = 0;
                        break;
                    case 2:
                        r = 0;
                        g = 255;
                        b = lo;
                        break;
                    case 3:
                        r = 0;
                        g = 255 - lo;
                        b = 255;
                        break;
                    case 4:
                        r = lo;
                        g = 0;
                        b = 255;
                        break;
                    default:
                        r = 255;
                        g = 0;
                        b = 255 - lo;
                        break;
                }

                var bright = (int)(Math.Pow(0.5 + Math.Sin(sine2) * 0.5, 2.8) * 255.0);
                ledArray[i++] = (byte)((r * bright) / 255);
                ledArray[i++] = (byte)((g * bright) / 255);
                ledArray[i++] = (byte)((b * bright) / 255);

                hue2 += 40;
                sine2 += 0.3f;
            }

            _hue = (_hue + 4) % 1536;
            _sine -= .03f;

            return true;
        }
    }
}