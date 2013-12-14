using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaLightNetShell
{
    public static class LedConstants
    {
        public const byte LED_COUNT = 25;
        public const byte BYTES_PER_LED = 3;
        public const byte LED_ARRAY_SIZE = LED_COUNT * BYTES_PER_LED;

        public const int TICK_EVERY_MILISEC = 10;
    }
}
