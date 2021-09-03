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
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.System;
using LightParty.LightController;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LightParty.Pages.PartyMode.Simple
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PartyControlSimple : Page
    {
        PartyControl partyControl;

        public PartyControlSimple()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SelectLastSaveButton();
        }

        public void GiveVariables(PartyControl newPartyControl)
        {
            partyControl = newPartyControl;
        }

        public void LightSelectionChanged()
        {

        }

        private void SelectLastSaveButton()
        {
            int id = PartyOptions.CompareCurrentWithSaves();

            if (id != -1)
            {
                SelectSaveButton(id);
            }
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).Tag);
            PartyOptions.SetPartyOption(id);
            SelectSaveButton(id);
        }

        private void SelectSaveButton(int id)
        {
            for (int i = 0; i < PartySavesGrid.Children.Count; i++)
            {
                Button button = PartySavesGrid.Children[i] as Button;
                Border border = button.Content as Border;

                border.BorderThickness = new Thickness(0);
            }

            Button saveButton = PartySavesGrid.Children[id] as Button;
            Border saveBorder = saveButton.Content as Border;

            saveBorder.BorderThickness = new Thickness(2);

            switch (id)
            {
                default:
                    break;
                case 0:
                    SaveOneDeactivated();
                    SaveZeroActivated();
                    break;
                case 1:
                    SaveZeroDeactivated();
                    SaveOneActivated();
                    break;
            }
            PartyOptions.useRGBColor = LightInformation.IsInRGBMode();
            PartyOptions.useMixedColorSpectrums = LightInformation.IsInMixedColorSpectrumsMode();
        }

        private async void SaveZeroActivated()
        {
            PartyUIUpdater.GiveVariablesSlider<PartyControlSimple, PartyControlSimple>(this, null);
            await SoundInput.StartMicrophoneInputSafely();
        }

        private void SaveZeroDeactivated()
        {
            SoundInput.StopMicrophoneInputSafely();
            PartyUIUpdater.GiveVariablesInterval<PartyControlSimple, PartyControlSimple>(null, null);
        }

        private void SaveOneActivated()
        {
            if (!LightProcessingRandom.isUpdating)
                LightProcessingRandom.StartUpdates();

            PartyUIUpdater.GiveVariablesInterval<PartyControlSimple, PartyControlSimple>(this, null);
        }

        private void SaveOneDeactivated()
        {
            LightProcessingRandom.StopUpdates();
        }

        public void SetMicrophoneInputSlider(double newValue)
        {
            MicrophoneInputSlider.Value = newValue;
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

        private void MoreOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            partyControl.SelectMenuItem("Advanced");
            partyControl.NavigateToItem("Advanced");
        }

        public void StopActiveProcesses()
        {
            SoundInput.StopMicrophoneInputSafely();
            LightProcessingRandom.StopUpdates();
        }
    }
}
