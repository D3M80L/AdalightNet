using System;
using System.Threading;
using AdaLightNetShell.Generators;
using AdaLightNetShell.Infrastructure;
using AdaLightNetShell.LedServices;

namespace AdaLightNetShell
{
    public class Processor
    {
        private Timer _timer;
        private ILedGenerator _ledGenerator;
        private ILedService _ledService;
        private byte[] _ledArray = new byte[LedConstants.LED_ARRAY_SIZE];
        private ILedGenerator _generator;

        public void Run(ILedService ledService)
        {
            _ledService = ledService;
            _timer = new Timer(Tick);
            GC.KeepAlive(_timer);
            _timer.Change(200, LedConstants.TICK_EVERY_MILISEC);
        }

        public ILedGenerator Generator
        {
            get { return _generator; }
            set
            {
                if (_generator != null)
                {
                    _generator.Dispose();
                }
                _generator = value;
                if (_generator != null)
                {
                    _generator.Initialize();
                }
            }
        }

        private void Tick(object state)
        {
            try
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
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
    }
}