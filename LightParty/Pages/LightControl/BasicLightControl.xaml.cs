using LightParty.Services;
using Q42.HueApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using LightParty.LightController;
using System.Threading.Tasks;
using System.Diagnostics;
using mUi = Microsoft.UI.Xaml.Controls;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.Original;
using LightParty.Connection;
using Windows.UI.Xaml.Media.Animation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LightParty.Pages.LightControl
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BasicLightControl : Page
    {
        private mUi.NavigationViewItem rgbColorPickerNavItem;
        private bool colorPickerNavInvokeItems = true;

        public BasicLightControl()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            rgbColorPickerNavItem = ColorPickerNav.MenuItems[0] as mUi.NavigationViewItem;

            LightSelectionFrame.Navigate(typeof(LightSelection));
            ((LightSelection)LightSelectionFrame.Content).GiveVariables(this);
        }

        public async Task LightSelectionChanged()
        {
            if (BridgeInformation.usedLights.Count > 0)
            {
                InformationGird.Opacity = 0;
                await Task.Delay((int)InformationGird.OpacityTransition.Duration.TotalMilliseconds);
                InformationGird.Visibility = Visibility.Collapsed;

                UserControlGrid.Visibility = Visibility.Visible;
                UserControlGrid.Opacity = 1;

                UpdateUserControls();
            }
            else
            {
                UserControlGrid.Opacity = 0;
                await Task.Delay((int)UserControlGrid.OpacityTransition.Duration.TotalMilliseconds);
                UserControlGrid.Visibility = Visibility.Collapsed;

                InformationGird.Visibility = Visibility.Visible;
                InformationGird.Opacity = 1;

                BasicLightController.canControl = false;
            }
        }

        private void UpdateUserControls()
        {
            if (BridgeInformation.usedLights.Count > 0)
            {
                SetLightToggleSwitch();
                SetBrightnessSlider();
                SetColorPicker();

                BasicLightController.canControl = true;
            }
        }

        public async Task UserControlUsed()
        {
            await Task.Delay(10);
            _ = ((LightSelection)LightSelectionFrame.Content).UpdateLightButtons();
        }

        private void SetIsEnabled()
        {
            if (LightToggleSwitch.IsOn)
            {
                BrightnessSlider.IsEnabled = true;
            }
            else
            {
                BrightnessSlider.IsEnabled = false;
            }

            SetIsEnabledColorPicker();
        }

        //Light Toggle

        private void SetLightToggleSwitch()
        {
            LightToggleSwitch.IsOn = BridgeInformation.mainLightTarget.State.On;
            SetIsEnabled();
        }

        private void LightToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch.IsOn)
            {
                BasicLightController.TurnOn();
            }
            else
            {
                BasicLightController.TurnOff();
            }

            SetIsEnabled();

            if (BasicLightController.canControl)
                _ = UserControlUsed();
        }

        //Brightness Slider

        void SetBrightnessSlider()
        {
            int brightness = LightInformation.GetBrightnessInPercent(BridgeInformation.mainLightTarget);
            BrightnessSlider.Value = brightness;
        }

        private void BrightnessSlider_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            SetBrightness();
        }

        private void BrightnessSlider_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            SetBrightness();
        }

        private void SetBrightness()
        {
            int brightness = Convert.ToInt32(BrightnessSlider.Value);
            BasicLightController.SetBrightness(brightness);

            if (BasicLightController.canControl)
                _ = UserControlUsed();
        }

        //Color Picker

        private void SetColorPicker()
        {
            bool rgbColorPicker = SetRGBColorPickerVisibility();
            mUi.NavigationViewItem currentItem = ColorPickerNav.SelectedItem as mUi.NavigationViewItem;

            if (rgbColorPicker)
            {
                if (currentItem.Content.ToString() == rgbColorPickerNavItem.Content.ToString())
                {
                    SetRGBColorPicker();
                }
                else
                {
                    rgbColorPicker = false;
                }
            }

            if (!rgbColorPicker)
            {
                colorPickerNavInvokeItems = false;

                SelectMenuItem("Temperature");
                NavigateToItem("Temperature");
                SetTemperatureColorPicker();

                colorPickerNavInvokeItems = true;
            }
        }

        private bool CheckRGBColorPickerVisibility()
        {
            bool show = true;

            foreach (Light light in BridgeInformation.lights)
            {
                if (BridgeInformation.usedLights.Contains(light.Id))
                {
                    if (LightInformation.IsTemperatureType(light))
                        show = false;
                }
            }

            return show;
        }

        private bool SetRGBColorPickerVisibility()
        {
            if (CheckRGBColorPickerVisibility())
            {
                if (!ColorPickerNav.MenuItems.Contains(rgbColorPickerNavItem))
                    ColorPickerNav.MenuItems.Insert(0, rgbColorPickerNavItem);

                mUi.NavigationViewItem currentItem = ColorPickerNav.SelectedItem as mUi.NavigationViewItem;
                if (currentItem == null)
                {
                    colorPickerNavInvokeItems = false;

                    ColorPickerNav.SelectedItem = ColorPickerNav.MenuItems.ElementAt(0);
                    NavigateToItem("RGB");

                    colorPickerNavInvokeItems = true;
                }

                return true;
            }
            else
            {
                if (ColorPickerNav.MenuItems.Contains(rgbColorPickerNavItem))
                    ColorPickerNav.MenuItems.RemoveAt(0);

                return false;
            }
        }

        private void SetIsEnabledColorPicker()
        {
            dynamic navItemScript;

            mUi.NavigationViewItem currentItem = ColorPickerNav.SelectedItem as mUi.NavigationViewItem;
            if (currentItem == null)
                return;
            if (currentItem.Content.ToString() == rgbColorPickerNavItem.Content.ToString())
            {
                navItemScript = ColorPickerFrame.Content as RGBColorPicker;
            }
            else
            {
                navItemScript = ColorPickerFrame.Content as TemperatureColorPicker;
            }

            navItemScript.SetIsEnabled(LightToggleSwitch.IsOn);
        }

        private void ColorPickerNav_ItemInvoked(mUi.NavigationView sender, mUi.NavigationViewItemInvokedEventArgs args)
        {
            if (colorPickerNavInvokeItems)
            {
                NavigateToItem(args.InvokedItem.ToString());
            }
        }

        private void SelectMenuItem(string itemName)
        {
            foreach(mUi.NavigationViewItem item in ColorPickerNav.MenuItems)
            {
                if (item.Content.ToString() == itemName)
                {
                    ColorPickerNav.SelectedItem = item;
                }
            }
        }

        private void NavigateToItem(string itemName)
        {
            BasicLightController.canControl = false;

            Type newColorPickerType;
            switch (itemName)
            {
                case "RGB":
                    newColorPickerType = typeof(RGBColorPicker);
                    break;
                case "Temperature":
                    newColorPickerType = typeof(TemperatureColorPicker);
                    break;
                default:
                    newColorPickerType = typeof(RGBColorPicker);
                    break;
            }

            bool showAnimation = true;
            if (ColorPickerFrame.CurrentSourcePageType == newColorPickerType)
                showAnimation = false;

            NavigateToPage(newColorPickerType, showAnimation);
            dynamic frameContent = Convert.ChangeType(ColorPickerFrame.Content, newColorPickerType);
            frameContent.GiveVariables<BasicLightControl>(this);

            switch (itemName)
            {
                case "RGB":
                    SetRGBColorPicker();
                    break;
                case "Temperature":
                    SetTemperatureColorPicker();
                    break;
                default:
                    break;
            }

            SetIsEnabled();
            BasicLightController.canControl = true;
        }

        public void NavigateToPage(Type pageType, bool showAnimation)
        {
            if (showAnimation)
                ColorPickerFrame.Navigate(pageType);
            else
                ColorPickerFrame.Navigate(pageType, null, new SuppressNavigationTransitionInfo());
        }

        //RGB Color Picker

        private void SetRGBColorPicker()
        {
            Color color = Color.FromArgb(255, 255, 255, 255);
            if (!LightInformation.IsTemperatureType(BridgeInformation.mainLightTarget))
            {
                color = ColorAssistant.ConvertRGBColorToColor(BridgeInformation.mainLightTarget.State.ToRGBColor());
            }

            ((RGBColorPicker)ColorPickerFrame.Content).SetRGBColorPickerColor(color);
        }

        public void SetRGBColor(Color color)
        {
            BasicLightController.SetRGBColor(new RGBColor(color.R, color.G, color.B));
            if (BasicLightController.canControl)
                _ = UserControlUsed();
        }

        //Temperture Color Picker

        private void SetTemperatureColorPicker()
        {
            int colorTemperature = 250;

            foreach (Light light in BridgeInformation.lights)
            {
                if (BridgeInformation.usedLights.Contains(light.Id))
                {
                    if (LightInformation.IsTemperatureType(light))
                    {
                        colorTemperature = Convert.ToInt32(light.State.ColorTemperature);
                    }
                    else
                    {
                        Color color = ColorAssistant.ConvertRGBColorToColor(light.State.ToRGBColor());
                        int temp = ColorAssistant.TryToConvertColorToColorTemperatur(color);

                        if (temp != -1)
                            colorTemperature = temp;
                    }
                }
            }

            ((TemperatureColorPicker)ColorPickerFrame.Content).SetColorTemperatureSliderPosition(colorTemperature);
        }

        public void SetColorTemperature(int colorTemperature)
        {
            BasicLightController.SetColorTemperature(colorTemperature);
            if (BasicLightController.canControl)
                _ = UserControlUsed();
        }
    }
}
