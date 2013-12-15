using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaLightNetShell.Infrastructure
{
    public static class Log
    {
        private static Action<string> _handler;
        public static void Register(Action<string> handler)
        {
            _handler = handler;
        }

        public static void Error(string message)
        {
            if (_handler != null)
            {
                _handler(string.Format("{0} - Error: {1}", DateTime.Now.TimeOfDay,  message));
            }
        }

        public static void Info(string message)
        {
            if (_handler != null)
            {
                _handler(string.Format("{0} - Info: {1}", DateTime.Now.TimeOfDay, message));
            }
        }
    }
}
