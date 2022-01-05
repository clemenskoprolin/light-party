using LightParty.LightController;
using LightParty.Pages.LightControl;
using LightParty.Party;
using LightParty.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace LightParty.Pages.PartyMode.Advanced
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PartyControlAdvanced : Page
    {
        PartyControl partyControl;
        dynamic colorOption;
        bool canSelect = false;

        public PartyControlAdvanced()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AudioSourceComboBox.SelectedIndex = PartyOptions.activePartyOption.audioSource;
            BrightnessOptionComboBox.SelectedIndex = PartyOptions.activePartyOption.brightnessOptionIndex;
            ColorOptionComboBox.SelectedIndex = PartyOptions.activePartyOption.colorOptionIndex;
            NavigateToBrighnessOption(PartyOptions.activePartyOption.brightnessOptionIndex);
            NavigateToColorOption(PartyOptions.activePartyOption.colorOptionIndex);

            canSelect = true;
            PartyUIUpdater.GiveVariablesOutput(this);
        }

        public void GiveVariables(PartyControl newPartyControl)
        {
            partyControl = newPartyControl;
        }

        public void LightSelectionChanged()
        {
            if (colorOption != null)
                colorOption.LightSelectionChanged();
        }

        //General
        private void AudioSourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (canSelect)
                NavigateToBrighnessOption(BrightnessOptionComboBox.SelectedIndex);
        }

        //Brightness

        private void BrightnessOptionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (canSelect)
                NavigateToBrighnessOption(BrightnessOptionComboBox.SelectedIndex);
        }

        private void NavigateToBrighnessOption(int index)
        {
            PartyOptions.activePartyOption.brightnessOptionIndex = index;

            switch (index)
            {
                case 0:
                    BrightnessOptionFrame.Visibility = Visibility.Visible;
                    BrightnessOptionFrame.Navigate(typeof(MicrophoneBrightnessOptions));
                    break;
                case 1:
                    BrightnessOptionFrame.Visibility = Visibility.Visible;
                    BrightnessOptionFrame.Navigate(typeof(RandomBrightnessOption));
                    break;
                case 2:
                    BrightnessOptionFrame.Visibility = Visibility.Collapsed;
                    break;
            }

            if (CheckIfSoundInputIsUsed())
                SelectedAudioInput();
            else
                UnselectedAudioInput();

            if (CheckIfRandomIsUsed())
                SelectedRandom();
            else
                UnselectedRandom();
        }

        private void ColorOptionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (canSelect)
                NavigateToColorOption(ColorOptionComboBox.SelectedIndex);
        }

        //Color

        private void NavigateToColorOption(int index)
        {
            PartyOptions.activePartyOption.colorOptionIndex = index;

            switch (index)
            {
                case 0:
                    ColorOptionFrame.Visibility = Visibility.Visible;
                    ColorOptionFrame.Navigate(typeof(MicrophoneColorOption));

                    ((MicrophoneColorOption)ColorOptionFrame.Content).GiveVariables(partyControl);
                    break;
                case 1:
                    ColorOptionFrame.Visibility = Visibility.Visible;
                    ColorOptionFrame.Navigate(typeof(MicrophoneDifferenceColorOption));

                    ((MicrophoneDifferenceColorOption)ColorOptionFrame.Content).GiveVariables(partyControl);
                    break;
                case 2:
                    ColorOptionFrame.Visibility = Visibility.Visible;
                    ColorOptionFrame.Navigate(typeof(RandomColorOption));
                    ((RandomColorOption)ColorOptionFrame.Content).GiveVariables(partyControl);
                    break;
                case 3:
                    ColorOptionFrame.Visibility = Visibility.Collapsed;
                    break;
            }

            if (CheckIfSoundInputIsUsed())
                SelectedAudioInput();
            else
                UnselectedAudioInput();

            if (CheckIfRandomIsUsed())
                SelectedRandom();
            else
                UnselectedRandom();

            if (ColorOptionFrame.Content != null)
                colorOption = Convert.ChangeType(ColorOptionFrame.Content, ColorOptionFrame.CurrentSourcePageType);
        }

        public async void StopActiveProcesses()
        {
            await AudioInput.StopAudioInputSafely();
            UnselectedRandom();
        }

        //Microphone input

        private async void SelectedAudioInput()
        {
            GiveAudioInputSlidersVariables();
            await AudioInput.StartAudioInputSafely();
        }

        private async void UnselectedAudioInput()
        {
            if (!CheckIfSoundInputIsUsed())
            {
                await AudioInput.StopAudioInputSafely();
            }

            GiveAudioInputSlidersVariables();
        }

        private void GiveAudioInputSlidersVariables()
        {
            MicrophoneBrightnessOptions brightnessOption = BrightnessOptionFrame.Content as MicrophoneBrightnessOptions;
            MicrophoneColorOption colorOption = ColorOptionFrame.Content as MicrophoneColorOption;

            PartyUIUpdater.GiveVariablesSlider<MicrophoneBrightnessOptions, MicrophoneColorOption>(brightnessOption, colorOption);
        }

        private bool CheckIfSoundInputIsUsed()
        {
            bool isUsed = PartyOptions.activePartyOption.brightnessOptionIndex == 0 || PartyOptions.activePartyOption.colorOptionIndex == 0 || PartyOptions.activePartyOption.colorOptionIndex == 1;
            return isUsed;
        }

        //random

        private void SelectedRandom()
        {
            if (!LightProcessingRandom.isUpdating)
                LightProcessingRandom.StartUpdates();

            RandomBrightnessOption randomBrightnessOption = BrightnessOptionFrame.Content as RandomBrightnessOption;
            RandomColorOption randomColorOption = ColorOptionFrame.Content as RandomColorOption;
            PartyUIUpdater.GiveVariablesInterval<RandomBrightnessOption, RandomColorOption>(randomBrightnessOption, randomColorOption);
        }

        private void UnselectedRandom()
        {
            LightProcessingRandom.StopUpdates();
        }

        private bool CheckIfRandomIsUsed()
        {
            return PartyOptions.activePartyOption.brightnessOptionIndex == 1 || PartyOptions.activePartyOption.colorOptionIndex == 2;
        }

        //Output Display

        public void UpdateOutputDisplay(int? brightness, Color? colorLeft, Color? colorRight)
        {
            if (brightness != null)
                BrightnessOutput.Value = Convert.ToDouble(brightness);

            if (colorLeft != null)
            {
                ColorOutputLeftStart.Color = (Color)colorLeft;
                ColorOutputLeftEnd.Color = (Color)colorLeft;
            }

            if(colorRight != null)
            {
                ColorOutputRight.Color = (Color)colorRight;
            }
        }
    }
}
