using AdaLightNetShell.Controls;

namespace AdaLightNetShell.LedServices
{
    public sealed class LedMatrixPreview : ILedService
    {
        public AdaLedMatrix LedMatrix { get; set; }

        public void Display(byte[] ledArray)
        {
            LedMatrix.Dispatcher.Invoke(() =>
            {
                int arrayHeader = 0;
                for (byte i = 0; i < LedConstants.LED_COUNT; i++)
                {
                    LedMatrix.SetLedColor(i, ledArray[arrayHeader], ledArray[++arrayHeader], ledArray[++arrayHeader]);
                    ++arrayHeader;
                }
            });
        }
    }
}
