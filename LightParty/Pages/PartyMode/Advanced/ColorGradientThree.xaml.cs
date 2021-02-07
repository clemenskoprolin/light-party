using LightParty.LightController;
using LightParty.Services;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LightParty.Pages.PartyMode.Advanced
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ColorGradientThree : Page
    {
        private dynamic popupClass;
        private dynamic onColorClass;

        private Color rgbStartColor;
        private Color rgbCenterColor;
        private Color rgbStopColor;
        private int startColorTemperature = 154;
        private int centerColorTemperature = 200;
        private int stopColorTemperature = 500;
        private bool? rgbMode = null;

        public ColorGradientThree()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateInputs();
        }

        public void GiveVariables<PopupType, OnColorType>(PopupType newPopupClass, OnColorType newOnColorClass, ColorGradientInformation colorGradientInformation)
        {
            popupClass = newPopupClass;
            onColorClass = newOnColorClass;

            rgbStartColor = colorGradientInformation.startColor;
            rgbCenterColor = colorGradientInformation.centerColor;
            rgbStopColor = colorGradientInformation.endColor;
            startColorTemperature = colorGradientInformation.startColorTemperature;
            centerColorTemperature = colorGradientInformation.centerColorTemperature;
            stopColorTemperature = colorGradientInformation.endColorTemperature;
        }

        public void UpdateInputs()
        {
            if (LightInformation.IsInRGBMode() && rgbMode != true)
            {
                SetColor(rgbStartColor, 0);
                SetColor(rgbCenterColor, 1);
                SetColor(rgbStopColor, 2);

                rgbMode = true;
                onColorClass.ColorGradientThreeChanged(StartColor.Color, CenterColor.Color, StopColor.Color);
            }
            if (!LightInformation.IsInRGBMode() && rgbMode != false)
            {
                SetColorTemperature(startColorTemperature, 0);
                SetColorTemperature(centerColorTemperature, 1);
                SetColorTemperature(stopColorTemperature, 2);

                rgbMode = false;
                onColorClass.ColorTemperatureGradientThreeChanged(Convert.ToInt32(StartColorButton.Content), Convert.ToInt32(CenterColorButton.Content), Convert.ToInt32(StopColorButton.Content));
            }
        }

        private void StartColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (LightInformation.IsInRGBMode())
                popupClass.OpenColorRGBPickerPopup<ColorGradientThree>(StartColor.Color, this, 0);
            else
                popupClass.OpenTemperatureColorPickerPopup<ColorGradientThree>(Convert.ToInt32(StartColorButton.Content), this, 0);
        }

        private void CenterColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (LightInformation.IsInRGBMode())
                popupClass.OpenColorRGBPickerPopup<ColorGradientThree>(CenterColor.Color, this, 1);
            else
                popupClass.OpenTemperatureColorPickerPopup<ColorGradientThree>(Convert.ToInt32(CenterColorButton.Content), this, 1);
        }

        private void StopColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (LightInformation.IsInRGBMode())
                popupClass.OpenColorRGBPickerPopup<ColorGradientThree>(StopColor.Color, this, 2);
            else
                popupClass.OpenTemperatureColorPickerPopup<ColorGradientThree>(Convert.ToInt32(StopColorButton.Content), this, 2);
        }

        public void SetColor(Color newColor, int id)
        {
            switch (id)
            {
                case 0:
                    StartColor.Color = newColor;
                    StartColorButton.Content = "#" + StartColor.Color.ToString().Substring(3);
                    break;
                case 1:
                    CenterColor.Color = newColor;
                    CenterColorButton.Content = "#" + CenterColor.Color.ToString().Substring(3);
                    break;
                case 2:
                    StopColor.Color = newColor;
                    StopColorButton.Content = "#" + StopColor.Color.ToString().Substring(3);
                    break;
            }

            if (rgbMode == true)
                onColorClass.ColorGradientThreeChanged(StartColor.Color, CenterColor.Color, StopColor.Color);
        }

        public void SetColorTemperature(int newColorTemperature, int id)
        {
            switch (id)
            {
                case 0:
                    StartColor.Color = ColorAssistant.ConvertColorTemperatureToColor(newColorTemperature);
                    StartColorButton.Content = newColorTemperature.ToString();
                    break;
                case 1:
                    CenterColor.Color = ColorAssistant.ConvertColorTemperatureToColor(newColorTemperature);
                    CenterColorButton.Content = newColorTemperature.ToString();
                    break;
                case 2:
                    StopColor.Color = ColorAssistant.ConvertColorTemperatureToColor(newColorTemperature);
                    StopColorButton.Content = newColorTemperature.ToString();
                    break;
            }

            if (rgbMode == false)
                onColorClass.ColorTemperatureGradientThreeChanged(Convert.ToInt32(StartColorButton.Content), Convert.ToInt32(CenterColorButton.Content), Convert.ToInt32(StopColorButton.Content));
        }
    }
}
