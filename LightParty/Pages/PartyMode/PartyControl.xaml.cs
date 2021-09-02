using LightParty.LightController;
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
using mUi = Microsoft.UI.Xaml.Controls;

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
            PartyOptions.useMixedColorSpectrums = LightInformation.IsInMixedColorSpectrumsMode();

            partyOptionsFrameContent.LightSelectionChanged();

            if (!LightInformation.CheckIfTurnedOn())
            {
                BasicLightController.TurnOn();
            }

            BasicLightController.canControl = true;
        }

        #region Party options navigation view

        private void PartyOptionsNav_ItemInvoked(mUi.NavigationView sender, mUi.NavigationViewItemInvokedEventArgs args)
        {
             NavigateToItem(args.InvokedItem.ToString());
        }

        public void SelectMenuItem(string itemName)
        {
            foreach (mUi.NavigationViewItem item in PartyOptionsNav.MenuItems)
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

        public async void OpenColorRGBPickerPopup<T>(Color defaultColor, T source, int idOnReturn)
        {
            Frame colorPickerFrame = new Frame();
            ContentDialog colorPickerPopup = new ContentDialog()
            {
                Title = "Color picker",
                Content = colorPickerFrame,
                PrimaryButtonText = "Apply",
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Primary
            };

            colorPickerFrame.Width = 235;

            colorPickerFrame.Navigate(typeof(RGBColorPicker), null, new SuppressNavigationTransitionInfo());
            ((RGBColorPicker)colorPickerFrame.Content).GiveVariables<PartyControl>(this);
            ((RGBColorPicker)colorPickerFrame.Content).SetRGBColorPickerColor(defaultColor);
            ((RGBColorPicker)colorPickerFrame.Content).SetIsEnabled(true);

            colorPickerSource = source;
            colorPickerSourceId = idOnReturn;
            isRGB = true;

            await ApplyColorPickerPopup(colorPickerPopup);
        }

        public async void OpenTemperatureColorPickerPopup<T>(int defaultTemperature, T source, int idOnReturn)
        {
            Frame colorPickerFrame = new Frame();
            ContentDialog colorPickerPopup = new ContentDialog()
            {
                Title = "Color picker",
                Content = colorPickerFrame,
                PrimaryButtonText = "Apply",
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Primary
            };

            colorPickerFrame.Width = 240;

            colorPickerFrame.Navigate(typeof(TemperatureColorPicker), null, new SuppressNavigationTransitionInfo());
            ((TemperatureColorPicker)colorPickerFrame.Content).GiveVariables<PartyControl>(this);
            ((TemperatureColorPicker)colorPickerFrame.Content).SetColorTemperatureSliderPosition(defaultTemperature);
            ((TemperatureColorPicker)colorPickerFrame.Content).SetIsEnabled(true);

            colorPickerSource = source;
            colorPickerSourceId = idOnReturn;
            isRGB = false;

            await ApplyColorPickerPopup(colorPickerPopup);
        }

        public void SetRGBColor(Color color)
        {
            currentColor = color;
        }

        public void SetColorTemperature(int colorTemperature)
        {
            currentColorTemperature = colorTemperature;
        }

        private async Task ApplyColorPickerPopup(ContentDialog colorPickerPopup)
        {
            ContentDialogResult result = await colorPickerPopup.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                if (isRGB)
                    colorPickerSource.SetColor(currentColor, colorPickerSourceId);
                else
                    colorPickerSource.SetColorTemperature(currentColorTemperature, colorPickerSourceId);
            }
        }

        #endregion

        public void StopActiveProcesses()
        {
            partyOptionsFrameContent.StopActiveProcesses();
        }
    }
}
