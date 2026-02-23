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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using LightParty.Connection;
using LightParty.Services;

namespace LightParty.Pages.Introduction
{
    /// <summary>
    /// This page contains legal information like links to the license and Privacy Policy.
    /// </summary>
    public sealed partial class LegalInformation : Page
    {
        public LegalInformation()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) {        }

        /// <summary>
        /// Is called when the previous button is pressed. Navigates back to explanations.
        /// </summary>
        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToExplanations();
        }

        /// <summary>
        /// Is called when the next button is pressed. If the checkbox is checked, proceeds to the main shell.
        /// </summary>
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)AgreeCheckbox.IsChecked)
            {
                // Persist that the user agreed to the privacy policy.
                // For backwards compatibility: any non-null value (true from old users, false from new) means the user has agreed.
                TelemetryService.SetUseTelemetry(false);
                NavigateToMainShell();
            }
            else
            {
                ShowCheckboxRequired();
            }
        }

        /// <summary>
        /// Navigates to Explanations with a slide from left effect.
        /// </summary>
        private void NavigateToExplanations()
        {
            this.Frame.Navigate(typeof(Explanations), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }

        /// <summary>
        /// Shows the user that the AgreeCheckbox isn't checked.
        /// </summary>
        private void ShowCheckboxRequired()
        {
            LegalPageStoryboard.Begin();

            var transform = AgreeBorder.TransformToVisual((UIElement)LegalScrollView.Content);
            var position = transform.TransformPoint(new Point(0, 0));
            LegalScrollView.ChangeView(null, position.Y, null, false);
        }

        /// <summary>
        /// Navigates back to the MainShell with a slide from right effect.
        /// </summary>
        private void NavigateToMainShell()
        {
            //If the application is in demo mode, showedDemoModeIntroduction is set to true so that the introduction isn't showed a second time.
            if (BridgeInformation.demoMode)
                BridgeInformation.showedDemoModeIntroduction = true;

            Frame mainShellFrame = Window.Current.Content as Frame;
            MainShell mainShell = mainShellFrame.Content as MainShell;
            mainShell.HideIntroduction();
            //Frame.Navigate(typeof(MainShell), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
}
