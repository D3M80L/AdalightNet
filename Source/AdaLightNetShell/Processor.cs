using System.Threading;
using AdaLightNetShell.Generators;
using AdaLightNetShell.LedServices;

namespace AdaLightNetShell
{
    public class Processor
    {
        private Timer _timer;
        private ILedGenerator _ledGenerator;
        private ILedService _ledService;
        private byte[] _ledArray = new byte[LedConstants.LED_ARRAY_SIZE];

        public void Run(ILedGenerator ledGenerator, ILedService ledService)
        {
            _ledGenerator = ledGenerator;
            _ledService = ledService;
            _timer = new Timer(Tick);
            _timer.Change(200, 10);
        }

        private void Tick(object state)
        {
            if (_ledGenerator.Generate(_ledArray))
            {
                _ledService.Display(_ledArray);
            }
        }

        public void Stop()
        {

        }
    }
}