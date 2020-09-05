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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LightParty.Pages.BridgeConfiguration
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RegisterApplication : Page
    {
        bool updateBridgeClickIcon = true;
        int visableImage = 0;

        string informationText = "";

        public RegisterApplication()
        {
            this.InitializeComponent();
        }


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _ = UpdateBridgeClickIcon();
            informationText = BridgeInformation.Text;
        }

        private async Task UpdateBridgeClickIcon()
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

            if (updateBridgeClickIcon)
                _ = UpdateBridgeClickIcon();
        }

        private async void ProceedButton_Click(object sender, RoutedEventArgs e)
        {
            ProceedButton.IsEnabled = false;

            if (await ConnectToBridge.RegisterApplication())
            {
                if (await ConnectToBridge.InitializeKey())
                {
                    Connection.BridgeInformation.redirectToLightControl = false;
                    Frame.Navigate(typeof(BridgeConfig));
                }
            } 
            else
            {
                await Task.Delay(1000);
                ProceedButton.IsEnabled = true;
            }
        }
    }
}
