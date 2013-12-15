namespace AdaLightNetShell.Generators
{
    public class LinearRainbowGenerator : LinearGradientGenerator
    {
        public override void Initialize()
        {
            ValueTable = new[]
            {
                0,
                100f,
                200f,
                300f,
                400f,
                500f,
                600f,
                700f
            };

            Colors = new[]
            {
                new byte[] {148, 0, 211},
                new byte[] {75, 0, 130},
                new byte[] {0, 0, 255},
                new byte[] {0, 128, 0},
                new byte[] {255, 255, 0},
                new byte[] {255, 128, 0},
                new byte[] {255, 128, 0},
                new byte[] {255, 0, 0},
            };

            base.Initialize();
        }

        public override bool Generate(byte[] ledArray)
        {
            if (CurrentValue > Max)
            {
                CurrentValue = Max;
                Step *= -1;
            }

            if (CurrentValue < 0)
            {
                CurrentValue = 0;
                Step *= -1;
            }

            return base.Generate(ledArray);
        }
    }
}