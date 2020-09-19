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
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LightParty.Pages.PartyMode.Advanced
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MicrophoneBrightnessOptions : Page
    {
        public MicrophoneBrightnessOptions()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateMircophoneInputRangeSelector();
        }

        private void UpdateMircophoneInputRangeSelector()
        {
            MircophoneInputRangeSelector.RangeMin = PartyOptions.minSoundLevel;
            MircophoneInputRangeSelector.RangeMax = PartyOptions.maxSoundLevel;
        }

        private void MircophoneInputRangeSelector_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PartyOptions.minSoundLevel = MircophoneInputRangeSelector.RangeMin;
            PartyOptions.maxSoundLevel = MircophoneInputRangeSelector.RangeMax;
        }

        private void MircophoneInputRangeSelector_ThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            PartyOptions.minSoundLevel = MircophoneInputRangeSelector.RangeMin;
            PartyOptions.maxSoundLevel = MircophoneInputRangeSelector.RangeMax;
        }

        public void SetMircophoneInputSlider(double newValue)
        {
            MircophoneInputSlider.Value = newValue;
        }

        private void StartWithZeroBrightnessInRangeCheckBox_Click(object sender, RoutedEventArgs e)
        {
            PartyOptions.startWithZeroInRange = (bool)((CheckBox)sender).IsChecked;
        }
    }
}
