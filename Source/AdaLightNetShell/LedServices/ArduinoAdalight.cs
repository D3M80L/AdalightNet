using System;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaLightNetShell.LedServices
{
    public class ArduinoAdalight : ILedService
    {
        private SerialPort _serialPort;
        private byte[] _adaHeader;

        public ArduinoAdalight()
        {
            _adaHeader = new byte[] 
            {
                (byte)'A',
                (byte)'d',
                (byte)'a',
                (byte)((LedConstants.LED_COUNT - 1) >> 8),
                (byte)((LedConstants.LED_COUNT - 1) & 0xff),
                0
            };
            _adaHeader[5] = (byte)(_adaHeader[3] ^ _adaHeader[4] ^ 0x55);

            _serialPort = new SerialPort("COM3", 115200);
            _serialPort.Open();
        }
        public void Display(byte[] ledArray)
        {
            _serialPort.Write(_adaHeader, 0, 6);
            _serialPort.Write(ledArray, 0, LedConstants.LED_ARRAY_SIZE);
        }
    }
}
