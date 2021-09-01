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
using Windows.UI.ViewManagement;
using Windows.UI;
using mUi = Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.Core;
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
        /// Configure the application by setting up the DebugLog, the titel bar and checking, reading and writing the configuration files.
        /// </summary>
        private async Task ConfigureApp()
        {
            DebugLogSetup();
            TitelBarSetup();

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
        /// If Connection.BridgeInformation.useDebugLog is true, the DebugLog will be created and the user will be notified of the path to the file by the DebugLogNotice.
        /// </summary>
        private void DebugLogSetup()
        {
            DebugLogNotice.Visibility = Connection.BridgeInformation.useDebugLog ? Visibility.Visible : Visibility.Collapsed;

            if (Connection.BridgeInformation.useDebugLog)
            {
                string pathDebugLog = DebugLog.CreateDebugLog();
                DebugLogNotice.Text += pathDebugLog;

                DebugLog.WriteToLog("Path to DebugLog: " + pathDebugLog, 0);
                DebugLog.WriteToLog("MainShell launched.", 0);
            }
        }

        /// <summary>
        /// Applies AppTitleBar and sets the colors of the title bar buttons according to the current theme. Thank you SFM61319 (https://github.com/SFM61319) for the idea!
        /// </summary>
        public void TitelBarSetup()
        {
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            Window.Current.SetTitleBar(AppTitleBar);

            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonForegroundColor = (Color?)Resources["SystemBaseHighColor"];
            titleBar.ButtonHoverBackgroundColor = (Color?)Resources["SystemBaseLowColor"];
            titleBar.ButtonHoverForegroundColor = (Color?)Resources["SystemBaseHighColor"];
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
        private void MainNav_ItemInvoked(mUi.NavigationView sender, mUi.NavigationViewItemInvokedEventArgs args)
        {
            int newPageIndex;
            if (!args.IsSettingsInvoked)
                newPageIndex = Convert.ToInt32(((mUi.NavigationViewItem)args.InvokedItemContainer).Tag);
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
            MainNav.SelectedItem = MainNav.MenuItems[pageIndex] as mUi.NavigationViewItem;
            NavigateToPage(pageIndex, animation);
        }

        /// <summary>
        /// Navigates the MainNav to a page by index.
        /// </summary>
        /// <param name="pageIndex">Index of the page which will be invoked</param>
        /// <param name="animation">[Optional, default = 'default'] Name of the animation with which the page will be invoked</param>
        private void NavigateToPage(int pageIndex, string animation = "default")
        {
            DiscardTeachingTips();

            if (navPageIndex == 2)
                ((PartyMode.PartyControl)MainFrame.Content).StopActiveProcesses();

            NavViewNavigateToPage(navItemPages[pageIndex], animation);
            navPageIndex = pageIndex;

            if (pageIndex < 3)
                MainNav.Header = ((mUi.NavigationViewItem)MainNav.MenuItems[pageIndex]).Content;
            else
                MainNav.Header = "Settings";
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
        /// Updates the header of the MainNav to a given the a given string.
        /// </summary>
        /// <param name="newHeader">The string to which the header will be updated.</param>
        public void UpdateMainNavHeader(string newHeader)
        {
            MainNav.Header = newHeader;
        }

        /// <summary>
        /// Draws the user the MainNavigationView to attention by open it and showing the first teaching tip.
        /// </summary>
        public void ShowNavigationViewTeachingTips()
        {
            MainNav.IsPaneOpen = true;
            NavigationViewTeachingTip1.IsOpen = true;
        }

        /// <summary>
        /// Once the first teaching tip is closed, the second one will be displayed.
        /// </summary>
        private void NavigationViewTeachingTip1_Closed(mUi.TeachingTip sender, mUi.TeachingTipClosedEventArgs args)
        {
            _ = OpenNewTeachingTip();
        }

        /// <summary>
        /// Opens a new teaching tip after a delay if the others are closed.
        /// </summary>
        /// <returns></returns>
        private async Task OpenNewTeachingTip()
        {
            await Task.Delay(3000);

            if (!NavigationViewTeachingTip1.IsOpen)
            {
                MainNav.IsPaneOpen = true;
                NavigationViewTeachingTip2.IsOpen = true;
            }
        }

        /// <summary>
        /// Closes any open teaching tip.
        /// </summary>
        private void DiscardTeachingTips()
        {
            NavigationViewTeachingTip1.IsOpen = false;
            NavigationViewTeachingTip2.IsOpen = false;
        }

        #endregion
    }
}
