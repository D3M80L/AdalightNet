using System;

namespace AdaLightNetShell.Generators
{
    public class RandomColorsGenerator : ILedGenerator
    {
        public void Dispose()
        {
        }

        private Random _rnd;
        private int _tick;
        public void Initialize()
        {
            _rnd = new Random();
            _tick = 0;
        }

        
        public bool Generate(byte[] ledArray)
        {
            ++_tick;
            if (_tick%100 != 0)
            {
                return true;
            }

            _tick = 0;

            int p = 0;
            for (int i = 0; i < LedConstants.LED_COUNT; ++i)
            {
                ledArray[p] = (byte)(_rnd.NextDouble() * 255);
                ledArray[++p] = (byte)(_rnd.NextDouble() * 255);
                ledArray[++p] = (byte)(_rnd.NextDouble() * 255);
                ++p;
            }

            return true;
        }
    }
}