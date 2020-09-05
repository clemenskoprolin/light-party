using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.System;
using Windows.UI.Xaml.Media.Animation;
using LightParty.Pages;
using LightParty.Services;

namespace LightParty.Pages
{
    /// <summary>
    /// This page is the main shell of Light Party. It is always active and contains the main navigation view and other frames to invoke the necessary pages.
    /// </summary>
    public sealed partial class MainShell : Page
    {
        public bool userCanUseNav = false; //Whether or not the user is able to use the MainNav.
        private int navPageIndex = 0; //The index of the current Page selected in the MainNav.
        private Type[] navItemPages = { typeof(BridgeConfiguration.BridgeConfig), typeof(LightControl.BasicLightControl), typeof(PartyMode.PartyControl), typeof(Settings.MainSettings) };
        //Contains all types of the pages used by the MainNav.

        private int f2KeyCount = 0; //Is increased by one when the user uses his F2 key. Used by DemoKeyPressed.

        public MainShell()
        {
            this.InitializeComponent();

            //Adds the event CoreWindow_KeyDown when the user presses a key on his keyboard.
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await ConfigureApp();
            NavigateToPageAndSelect(0);
        }

        #region Application events

        /// <summary>
        /// Is called when the user presses a key on his keyboard. Calls the corresponding methods.
        /// </summary>
        private async void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs e)
        {
            if (e.VirtualKey == VirtualKey.F2)
                await DemoKeyPressed();
        }

        /// <summary>
        /// Is called when the user presses his F2 key. When the key has been pressed 5 times, demo mode is activated or deactivated and application reloads.
        /// </summary>
        /// <returns></returns>
        private async Task DemoKeyPressed()
        {
            f2KeyCount++;

            if (f2KeyCount > 4)
            {
                f2KeyCount = 0;

                LightController.BasicLightController.ClearAllLights();
                BridgeConfigurationFile.ResetBridgeConfigurationTemporarily();
                ConfigurationFile.ResetConfigurationTemporarily();
                userCanUseNav = false;

                Connection.BridgeInformation.isConnected = false;
                Connection.BridgeInformation.demoMode = !Connection.BridgeInformation.demoMode;
                Connection.BridgeInformation.showedDemoModeIntroduction = false;
                await ConfigureApp();

                NavigateToPageAndSelect(0);
            }
        }

        #endregion
        #region Application configuration

        /// <summary>
        /// Configure the application by checking, reading and writing the configuration files.
        /// </summary>
        private async Task ConfigureApp()
        {
            if (await BridgeConfigurationFile.CheckForBridgeConfiguration())
            {
                if (!await BridgeConfigurationFile.ReadBridgeConfigurationFile())
                {
                    await BridgeConfigurationFile.UpdateBridgeConfigurationFile();
                }
            }
            else
            {
                await BridgeConfigurationFile.CreateBridgeConfiguration();
            }

            if (await ConfigurationFile.CheckForConfiguration())
            {
                if (!await ConfigurationFile.ReadConfigurationFile())
                {
                    await ConfigurationFile.UpdateConfigurationFile();
                }
            }
            else
            {
                await ConfigurationFile.CreateConfiguration();
            }
        }

        /// <summary>
        /// Makes the FullScreenFrame visible and navigates to Introduction.Explanations.
        /// </summary>
        public void ShowIntroduction()
        {
            FullScreenFrame.Visibility = Visibility.Visible;
            FullScreenFrame.Navigate(typeof(Introduction.Explanations));

            MainNav.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Hides the FullScreenFrame and navigates to BridgeConfiguration.
        /// </summary>
        public void HideIntroduction()
        {
            FullScreenFrame.Visibility = Visibility.Collapsed;
            MainNav.Visibility = Visibility.Visible;
            FullScreenFrame.Content = null;

            NavigateToPageAndSelect(0, "noanimation");
        }

        #endregion
        #region Main navigation view

        /// <summary>
        /// Is called when the user clicks on one item of the MainNav. When the user is allowed to use it, this method calls the corresponding methods.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MainNav_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            int newPageIndex;
            if (!args.IsSettingsInvoked)
                newPageIndex = Convert.ToInt32(((NavigationViewItem)args.InvokedItemContainer).Tag);
            else
                newPageIndex = 3;


            if (newPageIndex != navPageIndex)
            {
                if (userCanUseNav)
                {
                    NavigateToPage(newPageIndex);
                }
                else
                {
                    MainNav.SelectedItem = MainNav.MenuItems[0];
                }
            }
        }

        /// <summary>
        /// Navigates the MainNav to a page by index and selects the corresponding item.
        /// </summary>
        /// <param name="pageIndex">Index of the page which will be invoked</param>
        /// <param name="animation">[Optional, default = 'default'] Name of the animation with which the page will be invoked</param>
        public void NavigateToPageAndSelect(int pageIndex, string animation = "default")
        {
            MainNav.SelectedItem = MainNav.MenuItems[pageIndex] as NavigationViewItem;
            NavigateToPage(pageIndex, animation);
        }

        /// <summary>
        /// Navigates the MainNav to a page by index.
        /// </summary>
        /// <param name="pageIndex">Index of the page which will be invoked</param>
        /// <param name="animation">[Optional, default = 'default'] Name of the animation with which the page will be invoked</param>
        private void NavigateToPage(int pageIndex, string animation = "default")
        {
            if (navPageIndex == 2)
                ((PartyMode.PartyControl)MainFrame.Content).StopActiveProcesses();

            NavViewNavigateToPage(navItemPages[pageIndex], animation);
            navPageIndex = pageIndex;
        }


        /// <summary>
        /// Navigates the MainNav to a page by a given type.
        /// </summary>
        /// <param name="pageType">Type of the page which will be invoked</param>
        /// <param name="animation">[Optional, default = 'default'] Name of the animation with which the page will be invoked</param>
        private void NavViewNavigateToPage(Type pageType, string animation)
        {
            switch(animation)
            {
                default:
                    MainFrame.Navigate(pageType);
                    break;
                case "default":
                    MainFrame.Navigate(pageType);
                    break;
                case "noanimation":
                    MainFrame.Navigate(pageType, null, new SuppressNavigationTransitionInfo());
                    break;
            }
        }

        /// <summary>
        /// Draws the user the MainNavigationView to attention by open it or, depending if the MainNavigationView is already open, by show a black overlay animation.
        /// </summary>
        public async Task DrawNavigationViewToAttention()
        {
            double closedWidth = ((NavigationViewItem)MainNav.MenuItems[0]).ActualWidth;
            MainNav.IsPaneOpen = true;

            await Task.Delay(80);
            double openedWidth = ((NavigationViewItem)MainNav.MenuItems[0]).ActualWidth;

            if (closedWidth == openedWidth)
                _ = ShowBlackOverlayAnimation();
        }

        /// <summary>
        /// Fade the BlackOverlay in, waits for 2.5 seconds and then fades it out.
        /// </summary>
        private async Task ShowBlackOverlayAnimation()
        {
            BlackOverlay.Visibility = Visibility.Visible;
            BlackOverlay.Opacity = 1;

            await Task.Delay(2500);

            BlackOverlay.Opacity = 0;
            await Task.Delay((int)BlackOverlay.OpacityTransition.Duration.TotalMilliseconds);
            BlackOverlay.Visibility = Visibility.Collapsed;
        }

        #endregion
    }
}
