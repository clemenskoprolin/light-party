using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.Original;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using LightParty.Services;
using LightParty.LightController;
using System.Threading.Tasks;
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LightParty.Pages.LightControl
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RGBColorPicker : Page
    {
        dynamic parent;

        public RGBColorPicker()
        {
            this.InitializeComponent();
        }

        public void GiveVariables<T>(T newParent)
        {
            parent = newParent;
        }

        public void SetIsEnabled(bool isEnabled)
        {
            RGBColorRingPicker.IsEnabled = isEnabled;
            RGBColorPickerHex.IsEnabled = isEnabled;
        }

        public void SetRGBColorPickerColor(Color newColor)
        {
            RGBColorRingPicker.Color = newColor;
            RGBColorPickerHex.Color = newColor;
        }

        private void RGBColorRingPicker_ColorChange(ColorPicker sender, ColorChangedEventArgs e)
        {
            RGBColorPickerHex.Color = RGBColorRingPicker.Color;
            _ = RGBColorPickerSetChange();
        }

        private async Task RGBColorPickerSetChange()
        {
            Color savedColor = RGBColorRingPicker.Color;
            await Task.Delay(350);

            if (RGBColorRingPicker.Color == savedColor)
                SetColorRGB();
        }

        private void RGBColorPickerHex_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            RGBColorRingPicker.Color = RGBColorPickerHex.Color;
        }

        private void SetColorRGB()
        {
            Color color = RGBColorRingPicker.Color;
            parent.SetRGBColor(color);
        }
    }
}
