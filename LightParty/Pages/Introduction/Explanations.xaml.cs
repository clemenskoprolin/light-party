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
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace LightParty.Pages.Introduction
{
    /// <summary>
    /// This page gives the user an overview about the possibilities of Light Party.
    /// </summary>
    public sealed partial class Explanations : Page
    {
        private int pageIndex = 0; //The current index of the shown page.
        private List<Grid> pages; //List of all available pages

        public Explanations()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            pages = new List<Grid>() { WelcomePage, LiveSyncPage, ControlMultiplyLightsPage, DarkModePage };
        }

        /// <summary>
        /// Is called when the previous button is pressed. Moves the current page to the left and moves the previous page to it's default position.
        /// </summary>
        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            DeactivatePreviousNextButton();

            float xTo = (float)((Frame)Window.Current.Content).ActualWidth;
            _ = MovePageTo(pages[pageIndex], new Vector3(xTo, 0, 0));
            pageIndex--;

            float xFrom = -(float)((Frame)Window.Current.Content).ActualWidth;
            _ = MovePageFrom(pages[pageIndex], new Vector3(xFrom, 0, 0));

            ActivatePreviousNextButton();
            if (pageIndex == 0)
                PreviousButton.IsEnabled = false;
        }

        /// <summary>
        /// Is called when the next button is pressed. Moves the current page to the right and moves the next page to it's default position.
        /// </summary>
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            DeactivatePreviousNextButton();

            float xTo = -(float)((Frame)Window.Current.Content).ActualWidth;
            _ = MovePageTo(pages[pageIndex], new Vector3(xTo, 0, 0));

            if (pageIndex+1 != pages.Count)
            {
                pageIndex++;

                float xFrom = (float)((Frame)Window.Current.Content).ActualWidth;
                _ = MovePageFrom(pages[pageIndex], new Vector3(xFrom, 0, 0));

                if (pageIndex == 3)
                    _ = LightToDarkModeImage();

                ActivatePreviousNextButton();
            }
            else
            {
                NavigateToLegalInformation();
            }
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
        /// Fades LightActionsDarkImage in after 0.3 seconds.
        /// </summary>
        private async Task LightToDarkModeImage()
        {
            await Task.Delay(400);
            LightActionsDarkImage.Opacity = 1;
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
        /// Navigates to LegalInformation with a slide from right effect.
        /// </summary>
        private void NavigateToLegalInformation()
        {
            this.Frame.Navigate(typeof(LegalInformation), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
}
