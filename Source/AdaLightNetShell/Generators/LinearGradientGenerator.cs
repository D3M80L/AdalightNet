using System;
using SlimDX.Direct3D10;

namespace AdaLightNetShell.Generators
{
    public class LinearGradientGenerator : ILedGenerator
    {
        protected float Max;

        public float Step { get; set; }

        public float CurrentValue { get; set; }

        public void Dispose()
        {

        }

        public virtual void Initialize()
        {
            Step = 1f;
            CurrentValue = 0;

            Max = ValueTable[ValueTable.Length - 1];
        }

        protected float[] ValueTable = new[]
        {
            0,
            100f,
            200f,
            300f,
            400f,
            500f,
            600f,
            700f,
            800f,
        };

        protected byte[][] Colors = new[]
        {
            new byte[] {71, 22, 5},
            new byte[] {122, 59, 26},
            new byte[] {187, 116, 60},
            new byte[] {233, 184, 108},
            new byte[] {236, 200, 122},
            new byte[] {233, 184, 108},
            new byte[] {187, 116, 60},
            new byte[] {122, 59, 26},
            new byte[] {71, 22, 5},
        };

        public virtual bool Generate(byte[] ledArray)
        {

            if (CurrentValue > Max)
            {
                CurrentValue = 0;
            }

            Calculate(ledArray);

            CurrentValue += Step;

            return true;
        }

        private void Calculate(byte[] ledArray)
        {
            int left = 0, right = 0;

            for (int i = 0; i < ValueTable.Length; i++)
            {
                if (ValueTable[i] == CurrentValue)
                {
                    SetColors(ledArray, Colors[i][0], Colors[i][1], Colors[i][2]);
                    return;
                }

                if (ValueTable[i] < CurrentValue)
                {
                    left = i;
                }

                if (ValueTable[i] > CurrentValue)
                {
                    right = i;

                    var distance = ValueTable[right] - ValueTable[left];
                    var rightWeight = Math.Round((CurrentValue - ValueTable[left]) / distance, 4, MidpointRounding.AwayFromZero);
                    var leftWeight = 1 - rightWeight;

                    var r = (byte)(Colors[left][0]*leftWeight + Colors[right][0]*rightWeight);
                    var g = (byte)(Colors[left][1]*leftWeight + Colors[right][1]*rightWeight);
                    var b = (byte)(Colors[left][2]*leftWeight + Colors[right][2]*rightWeight);

                    SetColors(ledArray, r, g, b);
                    return;
                }
            }
        }

        private void SetColors(byte[] ledArray, byte r, byte g, byte b)
        {
            int p = 0;
            for (int i = 0; i < LedConstants.LED_COUNT; i++)
            {
                ledArray[p] = r;
                ledArray[++p] = g;
                ledArray[++p] = b;
                ++p;
            }
        }
    }
}