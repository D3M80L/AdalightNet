﻿using System;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdaLightNetShell.Infrastructure;

namespace AdaLightNetShell.LedServices
{
    public class ArduinoAdalightService : ILedService
    {
        private SerialPort _serialPort;
        private static byte[] _adaHeader;

        public static string PortName { get; set; }

        static ArduinoAdalightService()
        {
            PortName = "COM3";

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
        }

        public ArduinoAdalightService()
        {

        }

        public void Stop()
        {
            try
            {
                if (_serialPort != null)
                {
                    _serialPort.Close();
                    _serialPort.Dispose();
                    _serialPort = null;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        public void Run()
        {
            try
            {
                Stop();

                var serialPort = new SerialPort(PortName, 115200);
                serialPort.Open();

                _serialPort = serialPort;

                Log.Info(string.Format("Port {0} opened for communication.", PortName));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }            
        }

        public void Display(byte[] ledArray)
        {
            if (_serialPort == null) return;

            try
            {
                _serialPort.Write(_adaHeader, 0, 6);
                _serialPort.Write(ledArray, 0, LedConstants.LED_ARRAY_SIZE);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Stop();
            }
        }
    }
}
