using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AdaLightNetShell.Infrastructure
{
    public static class Metrics
    {
        private static Stopwatch _stopwatch = null;
        [Conditional("DEBUG")]
        public static void Start()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        [Conditional("DEBUG")]
        public static void Stop(string message = null)
        {
            _stopwatch.Stop();
            Console.WriteLine("Elapsed time: {0} {1}", _stopwatch.ElapsedMilliseconds, message);
        }
    }
}
