using System;
using AdaLightNetShell.Controls;

namespace AdaLightNetShell.LedServices
{
    public sealed class LedMatrixPreviewService : ILedService
    {
        public AdaLedArray LedArray { get; set; }

        public bool Enable { get; set; }

        public void Display(byte[] ledArray)
        {
            if (Enable)
            {
                try
                {
                    LedArray.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            int arrayHeader = 0;
                            for (byte i = 0; i < LedConstants.LED_COUNT; i++)
                            {
                                LedArray.SetLedColor(i, ledArray[arrayHeader], ledArray[++arrayHeader],
                                    ledArray[++arrayHeader]);
                                ++arrayHeader;
                            }
                        }
                        catch (Exception ex)
                        {
                            // TODO:
                        }
                    });
                }
                catch
                {
                    // TODO:
                }
            }
        }
    }
}
