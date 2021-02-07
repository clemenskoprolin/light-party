using LightParty.Party;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
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
using Windows.System;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LightParty.Pages.PartyMode.Advanced
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RandomColorOption : Page
    {
        PartyControl partyControl;
        dynamic colorGradient;

        bool canSelect = false;

        public RandomColorOption()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateControls();
            canSelect = true;
        }

        public void GiveVariables(PartyControl newPartyControl)
        {
            partyControl = newPartyControl;
        }

        private void UpdateControls()
        {
            if (!PartyOptions.activePartyOption.changeColorCompletelyRandom)
            {
                NavigateToRandomType(PartyOptions.activePartyOption.colorOptionIndex);
                RandomTypeComboBox.SelectedIndex = PartyOptions.activePartyOption.colorOptionIndex;
            }
            else
            {
                NavigateToRandomType(2);
                RandomTypeComboBox.SelectedIndex = 2;
            }
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
                    PartyOptions.activePartyOption.changeColorCompletelyRandom = true;
                    ColorGradientFrame.Visibility = Visibility.Collapsed;
                    break;
            }

            if (ColorGradientFrame.Visibility == Visibility.Visible)
            {
                PartyOptions.activePartyOption.changeColorCompletelyRandom = false;

                colorGradient = Convert.ChangeType(ColorGradientFrame.Content, ColorGradientFrame.CurrentSourcePageType);

                LightProcessingColor lightProcessingColor = new LightProcessingColor();
                colorGradient.GiveVariables<PartyControl, LightProcessingColor>(partyControl, lightProcessingColor, PartyOptions.GetColorGradientInformation());
            }
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
