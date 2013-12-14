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

        public void Run(ILedService ledService)
        {
            _ledService = ledService;
            _timer = new Timer(Tick);
            _timer.Change(200, LedConstants.TICK_EVERY_MILISEC);
        }

        public ILedGenerator Generator { get; set; }

        private void Tick(object state)
        {
            var generator = Generator;
            if (generator == null)
            {
                return;
            }
            if (generator.Generate(_ledArray))
            {
                _ledService.Display(_ledArray);
            }
        }
    }
}