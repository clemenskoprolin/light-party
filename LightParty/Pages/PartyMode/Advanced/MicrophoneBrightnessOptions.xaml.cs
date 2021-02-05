﻿using System;
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
using System.Diagnostics;
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
            UpdateControls();
        }

        private void UpdateControls()
        {
            Debug.WriteLine(PartyOptions.activePartyOption.minSoundLevel);
            MicrophoneInputRangeSelector.RangeMin = PartyOptions.activePartyOption.minSoundLevel;
            MicrophoneInputRangeSelector.RangeMax = PartyOptions.activePartyOption.maxSoundLevel;

            ((CheckBox)StartWithZeroBrightnessInRangeCheckBox).IsChecked = PartyOptions.activePartyOption.startWithZeroInRange;
        }

        private void MicrophoneInputRangeSelector_ValueChanged(object sender, Microsoft.Toolkit.Uwp.UI.Controls.RangeChangedEventArgs e)
        {
            PartyOptions.activePartyOption.minSoundLevel = MicrophoneInputRangeSelector.RangeMin;
            PartyOptions.activePartyOption.maxSoundLevel = MicrophoneInputRangeSelector.RangeMax;
        }

        private void StartWithZeroBrightnessInRangeCheckBox_Click(object sender, RoutedEventArgs e)
        {
            PartyOptions.activePartyOption.startWithZeroInRange = (bool)((CheckBox)sender).IsChecked;
        }

        public void SetMicrophoneInputSlider(double newValue)
        {
            MicrophoneInputSlider.Value = newValue;
        }
    }
}
