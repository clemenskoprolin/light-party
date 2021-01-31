using LightParty.LightController;
using LightParty.Party;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LightParty.Pages.PartyMode.Advanced
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MicrophoneColorOption : Page
    {
        PartyControl partyControl;
        dynamic colorGradient;

        bool canSelect = false;

        public MicrophoneColorOption()
        {
            this.InitializeComponent();
        }

        public void GiveVariables(PartyControl newPartyControl)
        {
            partyControl = newPartyControl;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            NavigateToColorGradient(0);
            canSelect = true;
        }

        public void LightSelectionChanged()
        {
            if (colorGradient != null)
                colorGradient.UpdateInputs();
        }

        private void ColorGradientComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (canSelect)
                NavigateToColorGradient(ColorGradientComboBox.SelectedIndex);
        }

        private void NavigateToColorGradient(int index)
        {
            switch (index)
            {
                case 0:
                    ColorGradientFrame.Navigate(typeof(ColorGradientTwo));
                    break;
                case 1:
                    ColorGradientFrame.Navigate(typeof(ColorGradientThree));
                    break;
            }

            colorGradient = Convert.ChangeType(ColorGradientFrame.Content, ColorGradientFrame.CurrentSourcePageType);

            LightProcessingColor lightProcessingColor = new LightProcessingColor();
            colorGradient.GiveVariables<PartyControl, LightProcessingColor>(partyControl, lightProcessingColor, PartyOptions.GetColorGradientInformation());
        }

        public void SetMircophoneInputSlider(double newValue)
        {
            MircophoneInputSlider.Value = newValue;
        }
    }
}
