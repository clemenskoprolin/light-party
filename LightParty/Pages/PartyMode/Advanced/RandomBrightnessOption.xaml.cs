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
using LightParty.Party;
using Windows.System;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LightParty.Pages.PartyMode.Advanced
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RandomBrightnessOption : Page
    {
        public RandomBrightnessOption()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateControls();
        }

        private void UpdateControls()
        {
            RandomInputRangeSelector.RangeStart = PartyOptions.activePartyOption.minRandomBrightness;
            RandomInputRangeSelector.RangeEnd = PartyOptions.activePartyOption.maxRandomBrightness;
        }

        private void RandomInputRangeSelector_ValueChanged(object sender, Microsoft.Toolkit.Uwp.UI.Controls.RangeChangedEventArgs e)
        {
            PartyOptions.activePartyOption.minRandomBrightness = Convert.ToInt32(RandomInputRangeSelector.RangeStart);
            PartyOptions.activePartyOption.maxRandomBrightness = Convert.ToInt32(RandomInputRangeSelector.RangeEnd);
        }

        public void SetRandomUpdateIntervalTextBox(float newValue)
        {
            RandomUpdateIntervalTextBox.Text = newValue.ToString();
        }

        private void RandomUpdateIntervalTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            LightProcessingRandom.SetUpdateInterval(RandomUpdateIntervalTextBox.Text);
        }

        private void RandomUpdateIntervalTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
                LightProcessingRandom.SetUpdateInterval(RandomUpdateIntervalTextBox.Text);
        }
    }
}
