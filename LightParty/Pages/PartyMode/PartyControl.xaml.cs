﻿using LightParty.LightController;
using LightParty.Pages.PartyMode;
using LightParty.Pages.LightControl;
using LightParty.Pages.PartyMode.Simple;
using LightParty.Pages.PartyMode.Advanced;
using LightParty.Party;
using LightParty.Services;
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
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace LightParty.Pages.PartyMode
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    public sealed partial class PartyControl : Page
    {
        private SoundInput soundInput = new SoundInput();
        private LightProcessingSoundInput lightProcessing = new LightProcessingSoundInput();

        private dynamic partyOptionsFrameContent;

        private dynamic colorPickerSource;
        private int colorPickerSourceId;
        private Color currentColor;
        private int currentColorTemperature;
        private bool isRGB = true;

        public PartyControl()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LightSelectionFrame.Navigate(typeof(LightSelection));
            ((LightSelection)LightSelectionFrame.Content).GiveVariables(this);

            if (PartyOptions.activePartyOption.Equals(default(PartyOption)))
                PartyOptions.SetPartyOption(0);

            int id = PartyOptions.CompareCurrentWithSaves();
            if (id != -1)
            {
                SelectMenuItem("Simple");
                NavigateToItem("Simple");
            } else
            {
                SelectMenuItem("Advanced");
                NavigateToItem("Advanced");
            }
        }

        public void LightSelectionChanged()
        {
            PartyOptions.useRGBColor = LightInformation.IsInRGBMode();
            partyOptionsFrameContent.LightSelectionChanged();

            if (!LightInformation.CheckIfTurnedOn())
            {
                BasicLightController.TurnOn();
            }
        }

        #region Party options navigation view

        private void PartyOptionsNav_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
             NavigateToItem(args.InvokedItem.ToString());
        }

        public void SelectMenuItem(string itemName)
        {
            foreach (NavigationViewItem item in PartyOptionsNav.MenuItems)
            {
                if (item.Content.ToString() == itemName)
                {
                    PartyOptionsNav.SelectedItem = item;
                }
            }
        }

        public void NavigateToItem(string itemName)
        {
            Type newPartyOption;
            switch (itemName)
            {
                case "Simple":
                    newPartyOption = typeof(PartyControlSimple);
                    break;
                case "Advanced":
                    newPartyOption = typeof(PartyControlAdvanced);
                    break;
                default:
                    newPartyOption = typeof(PartyControlSimple);
                    break;
            }

            bool showAnimation = true;
            if (PartyOptionsFrame.CurrentSourcePageType == newPartyOption)
                showAnimation = false;

            NavigateToPage(newPartyOption, showAnimation);
            partyOptionsFrameContent = Convert.ChangeType(PartyOptionsFrame.Content, newPartyOption);
            partyOptionsFrameContent.LightSelectionChanged();

            partyOptionsFrameContent.GiveVariables(this);
        }

        public void NavigateToPage(Type pageType, bool showAnimation)
        {
            if (showAnimation)
                PartyOptionsFrame.Navigate(pageType);
            else
                PartyOptionsFrame.Navigate(pageType, null, new SuppressNavigationTransitionInfo());
        }

        #endregion
        #region Color picker popup

        public void OpenColorRGBPickerPopup<T>(Color defaultColor, T source, int idOnReturn)
        {
            ColorPickerFrame.Width = 235;

            ColorPickerFrame.Navigate(typeof(RGBColorPicker), null, new SuppressNavigationTransitionInfo());
            ((RGBColorPicker)ColorPickerFrame.Content).GiveVariables<PartyControl>(this);
            ((RGBColorPicker)ColorPickerFrame.Content).SetRGBColorPickerColor(defaultColor);
            ((RGBColorPicker)ColorPickerFrame.Content).SetIsEnabled(true);

            colorPickerSource = source;
            colorPickerSourceId = idOnReturn;
            isRGB = true;

            ColorPickerPopup.Visibility = Visibility.Visible;
            ColorPickerPopup.Scale = new Vector3(1, 1, 1);
        }

        public void OpenTemperatureColorPickerPopup<T>(int defaultTemperature, T source, int idOnReturn)
        {
            ColorPickerFrame.Width = 240;

            ColorPickerFrame.Navigate(typeof(TemperatureColorPicker), null, new SuppressNavigationTransitionInfo());
            ((TemperatureColorPicker)ColorPickerFrame.Content).GiveVariables<PartyControl>(this);
            ((TemperatureColorPicker)ColorPickerFrame.Content).SetColorTemperatureSliderPosition(defaultTemperature);
            ((TemperatureColorPicker)ColorPickerFrame.Content).SetIsEnabled(true);

            colorPickerSource = source;
            colorPickerSourceId = idOnReturn;
            isRGB = false;

            ColorPickerPopup.Visibility = Visibility.Visible;
            ColorPickerPopup.Scale = new Vector3(1, 1, 1);
        }

        public void SetRGBColor(Color color)
        {
            currentColor = color;
        }

        public void SetColorTemperature(int colorTemperature)
        {
            currentColorTemperature = colorTemperature;
        }

        private async void ApplyColorPickerPopup_Click(object sender, RoutedEventArgs e)
        {
            if (isRGB)
                colorPickerSource.SetColor(currentColor, colorPickerSourceId);
            else
                colorPickerSource.SetColorTemperature(currentColorTemperature, colorPickerSourceId);

            ColorPickerPopup.Scale = new Vector3(0, 0, 0);
            await Task.Delay((int)ColorPickerPopup.ScaleTransition.Duration.TotalMilliseconds);
            ColorPickerPopup.Visibility = Visibility.Collapsed;
        }

        private async void CancelColorPickerPopup_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerPopup.Scale = new Vector3(0, 0, 0);
            await Task.Delay((int)ColorPickerPopup.ScaleTransition.Duration.TotalMilliseconds);
            ColorPickerPopup.Visibility = Visibility.Collapsed;
        }

        #endregion

        public void StopActiveProcesses()
        {
            partyOptionsFrameContent.StopActiveProcesses();
        }
    }
}
