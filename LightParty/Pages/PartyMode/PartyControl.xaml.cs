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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LightParty.Pages.PartyMode
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    public sealed partial class PartyControl : Page
    {
        SoundInput soundInput = new SoundInput();
        LightProcessingSoundInput lightProcessing = new LightProcessingSoundInput();

        dynamic colorOption;
        bool canSelect = false;

        dynamic colorPickerSource;
        int colorPickerSourceId;
        Color currentColor;
        int currentColorTemperature;
        bool isRGB = true;

        public PartyControl()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LightSelectionFrame.Navigate(typeof(LightSelection));
            ((LightSelection)LightSelectionFrame.Content).GiveVariables(this);

            BrightnessOptionComboBox.SelectedIndex = PartyOptions.brightnessOptionIndex;
            ColorOptionComboBox.SelectedIndex = PartyOptions.colorOptionIndex;
            NavigateToBrighnessOption(PartyOptions.brightnessOptionIndex);
            NavigateToColorOption(PartyOptions.colorOptionIndex);

            canSelect = true;
            BasicLightController.canControl = true;

            PartyUIUpdater.GiveVariablesOutput(this);
        }

        public void LightSelectionChanged()
        {
            colorOption.LightSelectionChanged();

            if (!LightInformation.CheckIfTurnedOn())
            {
                BasicLightController.TurnOn();
            }
        }

        //Brightness

        private void BrightnessOptionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (canSelect)
                NavigateToBrighnessOption(BrightnessOptionComboBox.SelectedIndex);
        }

        private void NavigateToBrighnessOption(int index)
        {
            PartyOptions.brightnessOptionIndex = index;

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
                SelectedMircophoneInput();
            else
                UnselectedMircrophoneInput();

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
            PartyOptions.colorOptionIndex = index;

            switch (index)
            {
                case 0:
                    ColorOptionFrame.Visibility = Visibility.Visible;
                    ColorOptionFrame.Navigate(typeof(MicrophoneColorOption));

                    ((MicrophoneColorOption)ColorOptionFrame.Content).GiveVariables(this);
                    break;
                case 1:
                    ColorOptionFrame.Visibility = Visibility.Visible;
                    ColorOptionFrame.Navigate(typeof(MicrophoneDifferenceColorOption));

                    ((MicrophoneDifferenceColorOption)ColorOptionFrame.Content).GiveVariables(this);
                    break;
                case 2:
                    ColorOptionFrame.Visibility = Visibility.Visible;
                    ColorOptionFrame.Navigate(typeof(RandomColorOption));
                    ((RandomColorOption)ColorOptionFrame.Content).GiveVariables(this);
                    break;
                case 3:
                    ColorOptionFrame.Visibility = Visibility.Collapsed;
                    break;
            }

            if (CheckIfSoundInputIsUsed())
                SelectedMircophoneInput();
            else
                UnselectedMircrophoneInput();

            if (CheckIfRandomIsUsed())
                SelectedRandom();
            else
                UnselectedRandom();

            colorOption = Convert.ChangeType(ColorOptionFrame.Content, ColorOptionFrame.CurrentSourcePageType);
        }

        public void OpenColorRGBPickerPopup<T>(Color defaultColor, T source, int idOnReturn)
        {
            ColorPickerFrame.Width = 235;

            ColorPickerFrame.Navigate(typeof(RGBColorPicker), null, new SuppressNavigationTransitionInfo());
            ((RGBColorPicker)ColorPickerFrame.Content).GiveVariables<PartyControl>(this);
            ((RGBColorPicker)ColorPickerFrame.Content).SetRGBColorPickerColor(defaultColor);
            ((RGBColorPicker)ColorPickerFrame.Content).SetIsEnabled(true);

            colorPickerSource = source;
            colorPickerSourceId = idOnReturn;
            isRGB = true;

            ColorPickerPopup.Visibility = Visibility.Visible;
            ColorPickerPopup.Scale = new Vector3(1, 1, 1);
        }

        public void OpenTemperatureColorPickerPopup<T>(int defaultTemperature, T source, int idOnReturn)
        {
            ColorPickerFrame.Width = 240;

            ColorPickerFrame.Navigate(typeof(TemperatureColorPicker), null, new SuppressNavigationTransitionInfo());
            ((TemperatureColorPicker)ColorPickerFrame.Content).GiveVariables<PartyControl>(this);
            ((TemperatureColorPicker)ColorPickerFrame.Content).SetColorTemperatureSliderPosition(defaultTemperature);
            ((TemperatureColorPicker)ColorPickerFrame.Content).SetIsEnabled(true);

            colorPickerSource = source;
            colorPickerSourceId = idOnReturn;
            isRGB = false;

            ColorPickerPopup.Visibility = Visibility.Visible;
            ColorPickerPopup.Scale = new Vector3(1, 1, 1);
        }

        public void SetRGBColor(Color color)
        {
            currentColor = color;
        }

        public void SetColorTemperature(int colorTemperature)
        {
            currentColorTemperature = colorTemperature;
        }

        private async void ApplyColorPickerPopup_Click(object sender, RoutedEventArgs e)
        {
            if (isRGB)
                colorPickerSource.SetColor(currentColor, colorPickerSourceId);
            else
                colorPickerSource.SetColorTemperature(currentColorTemperature, colorPickerSourceId);

            ColorPickerPopup.Scale = new Vector3(0, 0, 0);
            await Task.Delay((int)ColorPickerPopup.ScaleTransition.Duration.TotalMilliseconds);
            ColorPickerPopup.Visibility = Visibility.Collapsed;
        }

        private async void CancelColorPickerPopup_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerPopup.Scale = new Vector3(0, 0, 0);
            await Task.Delay((int)ColorPickerPopup.ScaleTransition.Duration.TotalMilliseconds);
            ColorPickerPopup.Visibility = Visibility.Collapsed;
        }

        public void StopActiveProcesses()
        {
            StopMircophoneInput();
            UnselectedRandom();
        }

        //mircophone input

        private async void SelectedMircophoneInput()
        {
            GiveMicrophoneInputSlidersVariables();
            await StartMircophoneInput();
        }

        private void UnselectedMircrophoneInput()
        {
            if (!CheckIfSoundInputIsUsed())
            {
                StopMircophoneInput();
            }

            GiveMicrophoneInputSlidersVariables();
        }

        private async Task StartMircophoneInput()
        {
            if (!SoundInput.isListing && !SoundInput.isCreating)
            {
                SoundInput.stopOnCreation = false;
                await SoundInput.StartInput();
            }
        }

        public void StopMircophoneInput()
        {
            if (SoundInput.isCreating)
            {
                SoundInput.stopOnCreation = true;
            }

            if (SoundInput.isListing && !SoundInput.isCreating)
            {
                _ = SoundInput.StopInput();
            }
        }

        private void GiveMicrophoneInputSlidersVariables()
        {
            MicrophoneBrightnessOptions brightnessOption = BrightnessOptionFrame.Content as MicrophoneBrightnessOptions;
            MicrophoneColorOption colorOption = ColorOptionFrame.Content as MicrophoneColorOption;

            PartyUIUpdater.GiveVariablesSlider<MicrophoneBrightnessOptions, MicrophoneColorOption>(brightnessOption, colorOption);
        }

        private bool CheckIfSoundInputIsUsed()
        {
            bool isUsed = PartyOptions.brightnessOptionIndex == 0 || PartyOptions.colorOptionIndex == 0 || PartyOptions.colorOptionIndex == 1;
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
            return PartyOptions.brightnessOptionIndex == 1 || PartyOptions.colorOptionIndex == 2;
        }

        //Output Display

        public void UpdateOutputDisplay(int? brightness, Color? color)
        {
            if (brightness != null)
                BrightnessOutput.Value = Convert.ToDouble(brightness);

            if (color != null)
                ColorOutput.Color = (Color)color;
        }
    }
}
