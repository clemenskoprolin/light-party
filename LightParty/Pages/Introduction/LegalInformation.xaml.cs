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
    /// This page contains legal information like links to the license and Privacy Policy and gives the user a choice to enable or disable telemetry.
    /// </summary>
    public sealed partial class LegalInformation : Page
    {
        private int pageIndex = 0; //The current index of the shown page.
        private List<Grid> pages; //List of all available pages

        public LegalInformation()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            pages = new List<Grid>() { LegalPage, TelemetryPage };
        }

        /// <summary>
        /// Is called when the previous button is pressed. Moves the current page to the left and moves the previous page to it's default position.
        /// </summary>
        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            DeactivatePreviousNextButton();

            float xTo = (float)((Frame)Window.Current.Content).ActualWidth;
            _ = MovePageTo(pages[pageIndex], new Vector3(xTo, 0, 0));

            if (pageIndex-1 >= 0)
            {
                pageIndex--;

                float xFrom = -(float)((Frame)Window.Current.Content).ActualWidth;
                _ = MovePageFrom(pages[pageIndex], new Vector3(xFrom, 0, 0));

                ActivatePreviousNextButton();
            }
            else
            {
                NavigateToExplanations();
            }
        }

        /// <summary>
        /// Is called when the next button is pressed. Moves the current page to the right and moves the next page to it's default position, if certain conditions are met.
        /// </summary>
        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            bool move = false;

            if (pageIndex == 0)
            {
                if ((bool)AgreeCheckbox.IsChecked)
                {
                    //This sends a one time request to koprolin.com. More information can be found in the class TelemetryService.
                    _ = TelemetryService.SendFirstStartTelemetry();
                    move = true;
                }
                else
                    ShowCheckboxRequired();
            }

            if (pageIndex == 1)
            {
                if (((bool)DisableTelemetryButton.IsChecked || (bool)EnableTelemetryButton.IsChecked))
                    move = true;
                else
                    ShowTelemetryOptionRequired();
            }


            if (move)
            {
                DeactivatePreviousNextButton();

                float xTo = -(float)((Frame)Window.Current.Content).ActualWidth;
                _ = MovePageTo(pages[pageIndex], new Vector3(xTo, 0, 0));

                if (pageIndex + 1 != pages.Count)
                {
                    pageIndex++;

                    float xFrom = (float)((Frame)Window.Current.Content).ActualWidth;
                    _ = MovePageFrom(pages[pageIndex], new Vector3(xFrom, 0, 0));

                    ActivatePreviousNextButton();
                }
                else
                {
                    await Task.Delay((int)pages[pageIndex].TranslationTransition.Duration.TotalMilliseconds);
                    NavigateToMainShell();
                }
            }
        }

        /// <summary>
        /// Moves a grid to a vector 3 translation.
        /// </summary>
        /// <param name="page">The grid</param>
        /// <param name="vector">The vector 3 translation</param>
        private async Task MovePageTo(Grid page, Vector3 vector)
        {
            page.Translation = vector;

            await Task.Delay((int)page.TranslationTransition.Duration.TotalMilliseconds);
            page.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Moves a grid from a vector 3 translation to it's default position.
        /// </summary>
        /// <param name="page">The grid</param>
        /// <param name="vector">The vector 3 translation</param>
        private async Task MovePageFrom(Grid page, Vector3 vector)
        {
            TimeSpan timeSpanSave = page.TranslationTransition.Duration;
            page.TranslationTransition.Duration = TimeSpan.Zero;
            page.Translation = vector;
            page.TranslationTransition.Duration = timeSpanSave;

            await Task.Delay(25);
            page.Visibility = Visibility.Visible;
            page.Translation = Vector3.Zero;
        }

        /// <summary>
        /// Deactivates the PreviousButton and the NextButton.
        /// </summary>
        private void DeactivatePreviousNextButton()
        {
            PreviousButton.IsEnabled = false;
            NextButton.IsEnabled = false;
        }

        /// <summary>
        /// Activates the PreviousButton and the NextButton.
        /// </summary>
        private void ActivatePreviousNextButton()
        {
            PreviousButton.IsEnabled = true;
            NextButton.IsEnabled = true;
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
        /// Is called when the user clicks on the DisableTelemetryButton. Unchecks the EnableTelemetryButton and disables telemetry.
        /// </summary>
        private void DisableTelemetryButton_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)((ToggleButton)sender).IsChecked)
            {
                EnableTelemetryButton.IsChecked = false;
                TelemetryService.SetUseTelemetry(false);
            }
        }

        /// <summary>
        /// Is called when the user clicks on the EnableTelemetryButton. Unchecks the DisableTelemetryButton and enables telemetry.
        /// </summary>
        private void EnableTelemetryButton_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)((ToggleButton)sender).IsChecked)
            {
                DisableTelemetryButton.IsChecked = false;
                TelemetryService.SetUseTelemetry(true);
            }
        }

        /// <summary>
        /// Shows the user that neither the DisableTelemetryButton or the EnableTelemetryButton are checked.
        /// </summary>
        private void ShowTelemetryOptionRequired()
        {
            TelemetryPageStoryboard.Begin();

            var transform = TelemetryBorder.TransformToVisual((UIElement)TelemetryScrollView.Content);
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
