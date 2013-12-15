using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using AdaLightNetShell.Generators;
using AdaLightNetShell.Infrastructure;
using AdaLightNetShell.LedServices;
using System.Windows;

namespace AdaLightNetShell
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LedMatrixPreviewService _ledMatrixPreviewService = new LedMatrixPreviewService();
        private ArduinoAdalightService _arduinoAdalightService = new ArduinoAdalightService();
        private bool _displayPreview;
        private int _selectedGenerator;
        private Processor _processor;

        public ObservableCollection<string> Logs { get; private set; }

        public List<string> ComPorts { get; private set; }

        public string ArduinoPort
        {
            get { return _arduinoPort; }
            set
            {
                _arduinoPort = value;
                ArduinoAdalightService.PortName = _arduinoPort;
            }
        }

        public MainWindow()
        {
            Log.Register(LogHandler);
            Logs = new ObservableCollection<string>();
            ComPorts = SerialPort.GetPortNames().ToList();
            ArduinoPort = ComPorts.FirstOrDefault();
            DataContext = this;
            InitializeComponent();

            var ledGenerator = new SlimDxScreenGenerator();
            //var ledGenerator = new RainbowGenerator();
            ledGenerator.Initialize();

            var wrapService = new WrapService();
            wrapService.Add(_arduinoAdalightService);

            _ledMatrixPreviewService.LedArray = LedArrayPreview;
            wrapService.Add(_ledMatrixPreviewService);

            _processor = new Processor();
            _processor.Run(wrapService);

            MinimizeToTray.Enable(this);
        }

        private void LogHandler(string message)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Logs.Insert(0, message);
                if (Logs.Count > 2000)
                {
                    Logs.Clear();
                }
            }));
        }

        private Dictionary<int, Func<ILedGenerator>> _generators = new Dictionary<int, Func<ILedGenerator>>
        {
            {0, ()=>null},
            {1, ()=>new RainbowGenerator()},
            {2, ()=>new SlimDxScreenGenerator()},
            {3, ()=>new SolidColorGenerator()},
            {4, ()=>new RandomColorsGenerator()},
            {5, ()=>new LinearGradientGenerator()},
            {6, ()=>new LinearRainbowGenerator()},
        };

        private string _arduinoPort;

        public int SelectedGenerator
        {
            get { return _selectedGenerator; }
            set
            {
                _selectedGenerator = value;

                _processor.Generator = _generators[_selectedGenerator]();
            }
        }

        public bool DisplayPreview
        {
            get { return _displayPreview; }
            set
            {
                _displayPreview = value;
                _ledMatrixPreviewService.Enable = _displayPreview;

                LedArrayPreview.Visibility = _ledMatrixPreviewService.Enable ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            _arduinoAdalightService.Run();
        }

        private void ArduinoClose_OnClick(object sender, RoutedEventArgs e)
        {
            _arduinoAdalightService.Stop();
        }
    }
}
