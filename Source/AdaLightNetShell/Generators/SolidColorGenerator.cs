namespace AdaLightNetShell.Generators
{
    public class SolidColorGenerator : ILedGenerator
    {
        public static byte R { get; set; }
        public static byte G { get; set; }
        public static byte B { get; set; }

        static SolidColorGenerator()
        {
            R = 76;
            G = 149;
            B = 245;
        }

        public void Initialize()
        {
            
        }

        public bool Generate(byte[] ledArray)
        {
            int p = 0;
            for (int i = 0; i < LedConstants.LED_COUNT;++i)
            {
                ledArray[p] = R;
                ledArray[++p] = G;
                ledArray[++p] = B;
                ++p;
            }

            return true;
        }

        public void Dispose()
        {
            
        }
    }
}