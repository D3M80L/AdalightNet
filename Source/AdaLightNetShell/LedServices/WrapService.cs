using System.Collections.Generic;

namespace AdaLightNetShell.LedServices
{
    public class WrapService : ILedService
    {
        private List<ILedService> _ledServices = new List<ILedService>();

        public void Add(ILedService ledService)
        {
            _ledServices.Add(ledService);
        }

        public void Display(byte[] ledArray)
        {
            foreach (var service in _ledServices)
            {
                service.Display(ledArray);
            }
        }
    }
}