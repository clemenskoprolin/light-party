using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using LightParty.Connection;

namespace LightParty.Pages.BridgeConfiguration
{
    /// <summary>
    /// This page tries to register the application by showing the user instructions.
    /// </summary>
    public sealed partial class RegisterApplication : Page
    {
        MainShell mainShell;
        int visableImage = 0; //Contains 0 when BridgeClickIcon1 is visable and 1 when BridgeClickIcon2 is visible.

        public RegisterApplication()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Frame mainShellFrame = Window.Current.Content as Frame;
            mainShell = mainShellFrame.Content as MainShell;

            mainShell.UpdateMainNavHeader("Register Application");

            _ = UpdateBridgeClickIcon();
            _ = UpdateAvailableBridges();
        }

        /// <summary>
        /// Changes the visablity of BridgeClickIcon1 and BridgeClickIcon2 every seconds, so that only one of time is visable at the same time.
        /// </summary>
        private async Task UpdateBridgeClickIcon()
        {
            await Task.Delay(1000);

            while (true)
            {
                switch (visableImage)
                {
                    case 0:
                        BridgeClickIcon1.Visibility = Visibility.Collapsed;
                        BridgeClickIcon2.Visibility = Visibility.Visible;
                        visableImage = 1;
                        break;
                    case 1:
                        BridgeClickIcon2.Visibility = Visibility.Collapsed;
                        BridgeClickIcon1.Visibility = Visibility.Visible;
                        visableImage = 0;
                        break;
                }

                await Task.Delay(1000);
            }
        }

        /// <summary>
        /// Trys to register this application every 5 seconds. If successful, it sets BridgeInformation.redirectToLightControl to false and navigates to BridgeConfig.
        /// </summary>
        private async Task UpdateAvailableBridges()
        {
            await Task.Delay(5000);

            while (true)
            {
                if (await ConnectToBridge.RegisterApplication())
                {
                    Connection.BridgeInformation.redirectToLightControl = false;
                    Frame.Navigate(typeof(BridgeConfig));
                    break;
                }

                await Task.Delay(5000);
            }
        }
    }
}
