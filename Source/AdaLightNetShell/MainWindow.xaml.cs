using AdaLightNetShell.Generators;
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
        public MainWindow()
        {
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
        }

        public int SelectedGenerator
        {
            get { return _selectedGenerator; }
            set
            {
                _selectedGenerator = value;

                if (_selectedGenerator == 0)
                {
                    _processor.Generator = null;
                    return;
                }

                if (_selectedGenerator == 1)
                {
                    var generator = new RainbowGenerator();
                    generator.Initialize();
                    _processor.Generator = generator;
                    return;
                }

                if (_selectedGenerator == 2)
                {
                    var generator = new SlimDxScreenGenerator();
                    generator.Initialize();
                    _processor.Generator = generator;
                    return;
                }
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
    }
}
