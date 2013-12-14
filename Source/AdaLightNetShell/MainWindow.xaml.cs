using AdaLightNetShell.Generators;
using AdaLightNetShell.LedServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AdaLightNetShell
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var ledGenerator = new SlimDxScreenGenerator();
            //var ledGenerator = new RainbowGenerator();
            ledGenerator.Initialize();
            
            var wrapService = new WrapService();
            wrapService.Add(new ArduinoAdalight());
            wrapService.Add(new LedMatrixPreview() { LedMatrix = LedArray });

            var handler = new Processor();
            handler.Run(ledGenerator, wrapService);
        }
    }
}
