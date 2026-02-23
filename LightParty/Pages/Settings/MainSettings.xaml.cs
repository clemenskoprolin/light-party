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
using LightParty.Services;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel;
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LightParty.Pages.Settings
{
    /// <summary>
    /// This page contains all important settings of the application.
    /// </summary>
    public sealed partial class MainSettings : Page
    {
        public MainSettings()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SetThemeRadioButton();
            SetApplicationInformationText();
        }

        #region Theme radio buttons

        /// <summary>
        /// Checks the correct theme radio button.
        /// </summary>
        private void SetThemeRadioButton()
        {
            switch (GetApplicationTheme())
            {
                case 0:
                    LightButton.IsChecked = true;
                    break;
                case 1:
                    DarkButton.IsChecked = true;
                    break;
                case 2:
                    WindowsDefault.IsChecked = true;
                    break;
            }
        }

        /// <summary>
        /// Gets the current theme of the applicaton.
        /// </summary>
        /// <returns>The current theme of the applicaton</returns>
        private int? GetApplicationTheme()
        {
            return ApplicationData.Current.LocalSettings.Values["themeSetting"] as int?;
        }

        private void LightButton_Click(object sender, RoutedEventArgs e)
        {
            _ = SetApplicationTheme(0);
        }

        private void DarkButton_Click(object sender, RoutedEventArgs e)
        {
            _ = SetApplicationTheme(1);
        }

        private void WindowsDefault_Click(object sender, RoutedEventArgs e)
        {
            _ = SetApplicationTheme(2);
        }

        /// <summary>
        /// Sets the application to a given theme. In order to take effect application-wide, the applciationo restarts.
        /// </summary>
        /// <param name="theme">The given theme in the Windows.UI.Xaml.ElementTheme Type</param>
        private async Task SetApplicationTheme(int theme)
        {
            FrameworkElement frameworkElement = Window.Current.Content as FrameworkElement;
            switch (theme)
            {
                case 0:
                    frameworkElement.RequestedTheme = ElementTheme.Light;
                    break;
                case 1:
                    frameworkElement.RequestedTheme = ElementTheme.Dark;
                    break;
                case 2:
                    frameworkElement.RequestedTheme = ElementTheme.Default;
                    break;
            }

            ApplicationData.Current.LocalSettings.Values["themeSetting"] = theme;

            await Task.Delay(500);
            await CoreApplication.RequestRestartAsync("");
        }

        #endregion

        #region Application information text

        /// <summary>
        /// Sets the version of ApplicationInformationText to the current app version.
        /// </summary>
        private void SetApplicationInformationText()
        {
            PackageVersion version = Package.Current.Id.Version;
            ApplicationInformationText.Text = string.Format("Light Party {0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        #endregion
    }
}
