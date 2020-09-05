using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using Windows.UI.Xaml.Navigation;
using LightParty.Connection;

namespace LightParty.Pages.BridgeConfiguration
{
    /// <summary>
    /// This page displays all available Philip Hue Bridges.
    /// </summary>
    public sealed partial class FindBridge : Page
    {
        string manualIPInput = "";

        public FindBridge()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _ = UpdateAvailableBridges();
        }

        /// <summary>
        /// Updates every 5 secondes the buttons of the available Bridges.
        /// </summary>
        private async Task UpdateAvailableBridges()
        {
            Q42.HueApi.Models.Bridge.LocatedBridge[] bridges = await ConnectToBridge.LocateAllBridges();

            while(true)
            {
                bridges = await ConnectToBridge.LocateAllBridges();

                HideAllAvailableBridges();
                ShowAvailableBridges(bridges);

                await Task.Delay(5000);
            }
        }

        /// <summary>
        /// Removes all buttons of the available Bridges.
        /// </summary>
        private void HideAllAvailableBridges()
        {
            AvailableBridgeButtonsGrid.Children.Clear();
            AvailableBridgeButtonsGrid.RowDefinitions.Clear();
        }

        /// <summary>
        /// Creates the buttons of the given Bridges.
        /// </summary>
        /// <param name="bridges">An array of the available Bridges. </param>
        private void ShowAvailableBridges(Q42.HueApi.Models.Bridge.LocatedBridge[] bridges)
        {
            int currentRow = 0;
            foreach (Q42.HueApi.Models.Bridge.LocatedBridge bridge in bridges)
            {
                Grid buttonGrid = new Grid()
                {
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridLength.Auto.Value, GridLength.Auto.GridUnitType) });
                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridLength.Auto.Value, GridLength.Auto.GridUnitType) });
                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                FontIcon fontIcon = new FontIcon()
                {
                    FontFamily = new FontFamily("Segoe MDL2 Assets"),
                    Glyph = "\xF158"
                };
                Grid.SetColumn(fontIcon, 0);
                buttonGrid.Children.Add(fontIcon);

                TextBlock ipTextBlock = new TextBlock()
                {
                    Margin = new Thickness(5, 0, -5, 0),
                    Text = bridge.IpAddress,
                    Style = (Style)Application.Current.Resources["BigInformation"]
                };
                Grid.SetColumn(ipTextBlock, 1);
                buttonGrid.Children.Add(ipTextBlock);

                Button newButton = new Button()
                {
                    Name = bridge.IpAddress + "BridgeButton",
                    Tag = bridge.IpAddress,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255))
                };

                AvailableBridgeButtonsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60, GridUnitType.Auto) });
                Grid.SetRow(newButton, currentRow);
                AvailableBridgeButtonsGrid.Children.Add(newButton);

                newButton.Content = buttonGrid;
                newButton.Click += BridgeButton_Click;

                currentRow++;
            }
        }

        private void BridgeButton_Click(object sender, RoutedEventArgs e)
        {
            string ipAddress = ((Button)sender).Tag.ToString();
            ConfigureIPAddress(ipAddress);
        }

        private void ManualIPInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            manualIPInput = ManualIPInput.Text;
        }

        private void ManualIPInputAcceptButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigureIPAddress(manualIPInput);
        }

        /// <summary>
        /// Trys to establish a connection with a given Bridge and navigates to RegisterApplication;
        /// </summary>
        /// <param name="ipAddress">The IP address of the Bridge</param>
        private void ConfigureIPAddress(string ipAddress)
        {
            if (ConnectToBridge.SelectBridge(ipAddress))
            {
                Frame.Navigate(typeof(RegisterApplication));
            }
        }
    }
}