using LightParty.LightController;
using Q42.HueApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI;
using System.Diagnostics;
using LightParty.Connection;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LightParty.Pages.LightControl
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TemperatureColorPicker : Page
    {
        dynamic parent;

        public TemperatureColorPicker()
        {
            this.InitializeComponent();
        }

        public void GiveVariables<T>(T newParent)
        {
            parent = newParent;
        }

        public void SetIsEnabled(bool isEnabled)
        {
            ColorTemperatureSlider.IsEnabled = isEnabled;
        }

        public void SetColorTemperatureSliderPosition(int newColorTemperature)
        {
            ColorTemperatureSlider.Value = newColorTemperature;
        }

        private void ColorTemperatureSlider_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            SetColorTemperature();
        }

        private void ColorTemperatureSlider_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            SetColorTemperature();
        }

        private void SetColorTemperature()
        {
            int colorTemperature = Convert.ToInt32(ColorTemperatureSlider.Value);

            parent.SetColorTemperature(colorTemperature);
        }
    }
}
