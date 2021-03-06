﻿using System;
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
using AdaLightNetShell;

namespace AdaLightNetShell.Controls
{
    /// <summary>
    /// Interaction logic for AdaLedArray.xaml
    /// </summary>
    public partial class AdaLedArray : UserControl
    {
        private LedControl[] _leds = new LedControl[LedConstants.LED_COUNT];

        private int[][] _ledGridPosition =
        {
            new [] { 3,5 },
            new [] { 2,5 },
            new [] { 1,5 },
            new [] { 0,5 },
            new [] { 0,4 },
            new [] { 0,3 },
            new [] { 0,2 },
            new [] { 0,1 },
            new [] { 0,0 },
            new [] { 1,0 },
            new [] { 2,0 },
            new [] { 3,0 },
            new [] { 4,0 },
            new [] { 5,0 },
            new [] { 6,0 },
            new [] { 7,0 },
            new [] { 8,0 },
            new [] { 8,1 },
            new [] { 8,2 },
            new [] { 8,3 },
            new [] { 8,4 },
            new [] { 8,5 },
            new [] { 7,5 },
            new [] { 6,5 },
            new [] { 5,5 },
        };

        public AdaLedArray()
        {
            InitializeComponent();
            DisplayRgbTable.IsChecked = true;
            FreezeRefresh.IsChecked = false;

            for (int i = 0; i < LedConstants.LED_COUNT; ++i)
            {
                var rectangle = _leds[i] = new LedControl();
                rectangle.SetValue(Grid.ColumnProperty, _ledGridPosition[i][0]);
                rectangle.SetValue(Grid.RowProperty, _ledGridPosition[i][1]);

                ArrayGrid.Children.Add(rectangle);
            }
        }

        public void SetLedColor(byte ledId, byte r, byte g, byte b)
        {
            _leds[ledId].SetLedColor(r,g,b);
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var led in _leds)
            {
                led.ValueTableVisibility = DisplayRgbTable.IsChecked
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private void RefreshFreeze_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var led in _leds)
            {
                led.FreezeRefresh = FreezeRefresh.IsChecked;
            }
        }
    }
}
