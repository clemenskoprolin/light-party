using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using LightParty.Connection;
using LightParty.Services;
using System.Numerics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LightParty.Pages.BridgeConfiguration
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BridgeConfig : Page
    {
        MainShell mainShell;

        public BridgeConfig()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Frame mainShellFrame = Window.Current.Content as Frame;
            mainShell = mainShellFrame.Content as MainShell;

            _ = PassOn();
        }

        private async Task PassOn()
        {
            if (!TelemetryService.HasTelemetryConfig() || (BridgeInformation.demoMode && !BridgeInformation.showedDemoModeIntroduction))
            {
                mainShell.ShowIntroduction();
                return;
            }

            if (BridgeInformation.isConnected)
            {
                FillWithInformation();

                mainShell.userCanUseNav = true;
                return;
            }

            if (!ConnectToBridge.HasIPAddress())
            {
                this.Frame.Navigate(typeof(FindBridge));
                return;
            }

            if (!ConnectToBridge.HasAppKey())
            {
                if (ConnectToBridge.SelectBridgeWithSavedIPAddress())
                {
                    this.Frame.Navigate(typeof(RegisterApplication));
                    return;
                }
            }

            if (!BridgeInformation.isConnected && !BridgeInformation.demoMode)
            {
                if (await ConnectToSavedBridge())
                {
                    _ = TelemetryService.SendTelemetryReport();
                    mainShell.userCanUseNav = true;

                    if (BridgeInformation.redirectToLightControl)
                    {
                        BridgeInformation.redirectToLightControl = false;
                        mainShell.NavigateToPageAndSelect(1);
                    }
                    else
                    {
                        FillWithInformation();
                    }
                    return;
                }
            }
        }

        private async Task<bool> ConnectToSavedBridge()
        {
            if (ConnectToBridge.SelectBridgeWithSavedIPAddress())
            {
                if (await ConnectToBridge.InitializeKey())
                {
                    return true;
                }
            }

            return false;
        }

        private async void FillWithInformation()
        {
            if (BridgeInformation.demoMode)
                return;

            var configuration = await BridgeInformation.client.GetConfigAsync();

            BridgeName.Text = configuration.Name;
            ModelId.Text = configuration.ModelId;
            IPAddress.Text = configuration.IpAddress;
            NetMask.Text = configuration.NetMask;
            MacAddress.Text = configuration.MacAddress;

            Thread.CurrentThread.CurrentCulture = new CultureInfo(Windows.System.UserProfile.GlobalizationPreferences.Languages[0].ToString());
            LocalTime.Text = configuration.LocalTime.ToString();
            SoftwareVersion.Text = configuration.SoftwareVersion;
            APIVersion.Text = configuration.ApiVersion;
        }

        private void ShowAPIKeyButton_Click(object sender, RoutedEventArgs e)
        {
            if (APIKeyPopup.Visibility != Visibility.Visible) 
            {
                APIKeyPopup.Visibility = Visibility.Visible;
                APIKeyPopup.Scale = new Vector3(1, 1, 1);
            }
        }

        private async void YesAPIPopup_Click(object sender, RoutedEventArgs e)
        {
            APIKeyPopup.Scale = new Vector3(0, 0, 0);
            await Task.Delay((int)APIKeyPopup.ScaleTransition.Duration.TotalMilliseconds);
            APIKeyPopup.Visibility = Visibility.Collapsed;
            ShowAPIKeyButton.Visibility = Visibility.Collapsed;

            APIKey.Text = ConnectToBridge.GetAppKey();
            APIKey.Visibility = Visibility.Visible;
        }

        private async void NoAPIPopup_Click(object sender, RoutedEventArgs e)
        {
            APIKeyPopup.Scale = new Vector3(0, 0, 0);
            await Task.Delay((int)APIKeyPopup.ScaleTransition.Duration.TotalMilliseconds);
            APIKeyPopup.Visibility = Visibility.Collapsed;
        }

        private void OpenNavigationView_Click(object sender, RoutedEventArgs e)
        {
            _ = mainShell.DrawNavigationViewToAttention();
        }
    }
}
