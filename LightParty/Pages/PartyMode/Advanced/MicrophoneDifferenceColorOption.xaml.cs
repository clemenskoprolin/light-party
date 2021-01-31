using LightParty.Party;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LightParty.Pages.PartyMode.Advanced
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MicrophoneDifferenceColorOption : Page
    {
        PartyControl partyControl;
        dynamic colorGradient;

        bool canSelect = false;

        public MicrophoneDifferenceColorOption()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            NavigateToRandomType(0);
            canSelect = true;

            PartyUIUpdaterAdvanced.GiveVariablesInputDifference<MicrophoneDifferenceColorOption>(this);
        }

        public void GiveVariables(PartyControl newPartyControl)
        {
            partyControl = newPartyControl;
        }

        public void LightSelectionChanged()
        {
            if (colorGradient != null)
                colorGradient.UpdateInputs();
        }

        private void RandomTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (canSelect)
                NavigateToRandomType(RandomTypeComboBox.SelectedIndex);
        }

        private void NavigateToRandomType(int index)
        {
            switch (index)
            {
                case 0:
                    ColorGradientFrame.Visibility = Visibility.Visible;
                    ColorGradientFrame.Navigate(typeof(ColorGradientTwo));
                    break;
                case 1:
                    ColorGradientFrame.Visibility = Visibility.Visible;
                    ColorGradientFrame.Navigate(typeof(ColorGradientThree));
                    break;
                case 2:
                    PartyOptions.changeColorCompletelyRandom = true;
                    ColorGradientFrame.Visibility = Visibility.Collapsed;
                    break;
            }

            if (ColorGradientFrame.Visibility == Visibility.Visible)
            {
                PartyOptions.changeColorCompletelyRandom = false;

                colorGradient = Convert.ChangeType(ColorGradientFrame.Content, ColorGradientFrame.CurrentSourcePageType);

                LightProcessingColor lightProcessingColor = new LightProcessingColor();
                colorGradient.GiveVariables<PartyControl, LightProcessingColor>(partyControl, lightProcessingColor, PartyOptions.GetColorGradientInformation());

                colorGradient.UpdateInputs();
            }
        }

        private void InputDifferenceTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateColorDifferencePercent();
        }

        private void InputDifferenceTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
                UpdateColorDifferencePercent();
        }

        private void UpdateColorDifferencePercent()
        {
            if (float.TryParse(InputDifferenceTextBox.Text, out float number))
            {
                if (number > 0.1f && number < 100)
                {
                    PartyOptions.colorDifferencePercent = number;
                }
            }

            InputDifferenceTextBox.Text = PartyOptions.colorDifferencePercent.ToString();
        }

        public void UpdateInputDifferenceText(double newInputDifference)
        {
            string input = newInputDifference.ToString("N1", CultureInfo.InvariantCulture);
            CurrentInputDifferenceText.Text = input;
        }
    }
}
