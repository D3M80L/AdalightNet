using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AdaLightNetShell.Controls
{
    /// <summary>
    /// Interaction logic for LedControl.xaml
    /// </summary>
    public partial class LedControl : UserControl
    {
        public LedControl()
        {
            InitializeComponent();

            LedRectangle.Fill = new SolidColorBrush(new Color());
        }

        public void SetLedColor(byte r, byte g, byte b)
        {
            if (FreezeRefresh)
            {
                return;
            }

            (LedRectangle.Fill as SolidColorBrush).Color = Color.FromRgb(r, g, b);
            RValue.Text = r.ToString();
            GValue.Text = g.ToString();
            BValue.Text = b.ToString();
        }

        public Visibility ValueTableVisibility
        {
            get { return ValueTable.Visibility; }
            set { ValueTable.Visibility = value; }
        }

        public bool FreezeRefresh { get; set; }
    }
}
