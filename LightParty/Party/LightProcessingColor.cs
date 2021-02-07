using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI;
using Q42.HueApi.ColorConverters;
using LightParty.LightController;
using LightParty.Services;
using System.Diagnostics;

namespace LightParty.Party
{
    /// <summary>
    /// Contains all methods which return a color and a used by the Party Mode. This class also contains the handler for the color gradients which are used by the Party Mode.
    /// </summary>
    class LightProcessingColor
    {
        #region Color gradients

        /// <summary>
        /// Gets a color from the color gradient which visible to the user.
        /// </summary>
        /// <param name="step">Indicates at which location the color is located in the color gradient. 0 - 1 as an float</param>
        /// <returns>A color as a ColorInformation Type</returns>
        public static ColorInformation SetColorGradienStep(float step)
        {
            if (PartyOptions.useRGBColor)
                return SetRGBColorGradientStep(step);
            else
                return SetColorTemperatuerGradientStep(step);
        }

        /// <summary>
        /// Gets a color in the RGB color spectrum from the color gradient which visible to the user.
        /// </summary>
        /// <param name="step">Indicates at which location the color is located in the color gradient. 0 - 1 as an float</param>
        /// <returns>A RGBColor as a ColorInformation Type</returns>
        private static ColorInformation SetRGBColorGradientStep(float step)
        {
            Color outputColor = new Color();

            switch (PartyOptions.activePartyOption.colorGradientCount)
            {
                case 2:
                    outputColor = ColorAssistant.GetStepBetweenTwoColors(PartyOptions.activePartyOption.startColor, PartyOptions.activePartyOption.endColor, step);
                    break;
                case 3:
                    outputColor = ColorAssistant.GetStepBetweenThreeColors(PartyOptions.activePartyOption.startColor, PartyOptions.activePartyOption.centerColor, PartyOptions.activePartyOption.endColor, step);
                    break;
            }

            RGBColor rgbColor = new RGBColor(outputColor.R, outputColor.G, outputColor.B);
            BasicLightController.SetRGBColorInCommonCommand(rgbColor);

            return new ColorInformation(rgbColor, null);
        }

        /// <summary>
        /// Gets a color temperature from the color gradient which visible to the user.
        /// </summary>
        /// <param name="step">Indicates at which location the color temperature is located in the color gradient. 0 - 1 as an float</param>
        /// <returns>A color temperature as integer from 153 to 500</returns>
        private static ColorInformation SetColorTemperatuerGradientStep(float step)
        {
            int outputColorTemperature = PartyOptions.activePartyOption.startColorTemperature;

            switch (PartyOptions.activePartyOption.colorGradientCount)
            {
                case 2:
                    outputColorTemperature = ColorAssistant.GetStepBetweenTwoColorTemperatures(PartyOptions.activePartyOption.startColorTemperature, PartyOptions.activePartyOption.endColorTemperature, step);
                    break;
                case 3:
                    outputColorTemperature = ColorAssistant.GetStepBetweenThreeColorTemperatures(PartyOptions.activePartyOption.startColorTemperature, PartyOptions.activePartyOption.centerColorTemperature, PartyOptions.activePartyOption.endColorTemperature, step);
                    break;
            }

            BasicLightController.SetColorTemperatureInCommonCommand(outputColorTemperature);

            return new ColorInformation(null, outputColorTemperature);
        }

        #endregion
        #region Random

        /// <summary>
        /// Gets a random color with the attributes selected in the UI.
        /// </summary>
        /// <returns>A color in the ColorInformation Type</returns>
        public static ColorInformation SetRandomColorFromUIInput()
        {
            ColorInformation colorInformation = new ColorInformation();

            if (!PartyOptions.activePartyOption.changeColorCompletelyRandom)
            {
                Random random = new Random();
                colorInformation = SetColorGradienStep((float)random.NextDouble());
            }
            else
            {
                colorInformation = SetRandomColor();
            }

            return colorInformation;
        }

        /// <summary>
        /// Gets a completely random color.
        /// </summary>
        /// <returns>A color as a ColorInformation Type</returns>
        public static ColorInformation SetRandomColor()
        {
            if (PartyOptions.useRGBColor)
                return SetRandomRGBColor();
            else
                return SetRandomColorTemperature();
        }

        /// <summary>
        /// Returns a random Color in the RGB color spectrum.
        /// </summary>
        /// <returns>A RGB color as a ColorInformation Type</returns>
        private static ColorInformation SetRandomRGBColor()
        {
            RGBColor color = ColorAssistant.GetRandomRGBColor();
            BasicLightController.SetRGBColorInCommonCommand(color);

            return new ColorInformation(color, null);
        }

        /// <summary>
        /// Returns a random color temperature.
        /// </summary>
        /// <returns>A color temperature as a ColorInformation Type</returns>
        private static ColorInformation SetRandomColorTemperature()
        {
            int temperature = ColorAssistant.GetRandomColorTemperature();
            BasicLightController.SetColorTemperatureInCommonCommand(temperature);

            return new ColorInformation(null, temperature);
        }

        #endregion
        #region Color gradient handler

        public void ColorGradientTwoChanged(Color startColor, Color endColor)
        {
            PartyOptions.useRGBColor = true;
            PartyOptions.activePartyOption.colorGradientCount = 2;

            PartyOptions.activePartyOption.startColor = startColor;
            PartyOptions.activePartyOption.endColor = endColor;
        }

        public void ColorTemperatureGradientTwoChanged(int startColorTemperature, int endColorTemperature)
        {
            PartyOptions.useRGBColor = false;
            PartyOptions.activePartyOption.colorGradientCount = 2;

            PartyOptions.activePartyOption.startColorTemperature = startColorTemperature;
            PartyOptions.activePartyOption.endColorTemperature = endColorTemperature;
        }

        public void ColorGradientThreeChanged(Color startColor, Color centerColor, Color endColor)
        {
            PartyOptions.useRGBColor = true;
            PartyOptions.activePartyOption.colorGradientCount = 3;

            PartyOptions.activePartyOption.startColor = startColor;
            PartyOptions.activePartyOption.centerColor = centerColor;
            PartyOptions.activePartyOption.endColor = endColor;
        }

        public void ColorTemperatureGradientThreeChanged(int startColorTemperature, int centerColorTemperature, int endColorTemperature)
        {
            PartyOptions.useRGBColor = false;
            PartyOptions.activePartyOption.colorGradientCount = 3;

            PartyOptions.activePartyOption.startColorTemperature = startColorTemperature;
            PartyOptions.activePartyOption.centerColorTemperature = centerColorTemperature;
            PartyOptions.activePartyOption.endColorTemperature = endColorTemperature;
        }

        #endregion
    }
}

/// <summary>
/// This struct can contain a color in the RGB color spectrum as well as a color temperature.
/// </summary>
public struct ColorInformation
{
    public RGBColor? rgbColor; //Color in the RGB color spectrum as a RGBColor Type
    public int? colorTemperature; //ColorTemperature as an integer.

    public ColorInformation(RGBColor? newRgbColor, int? newColorTemperature)
    {
        rgbColor = newRgbColor;
        colorTemperature = newColorTemperature;
    }
}

/// <summary>
/// This struct contains the color information of a color gradient.
/// </summary>
public struct ColorGradientInformation
{
    public Color startColor; //Start color of a RGB color spectrum color gradient.
    public Color centerColor; //Center color of a RGB color spectrum color gradient.
    public Color endColor; //Stop color of a RGB color spectrum color gradient.

    public int startColorTemperature; //Start color of a color temperature color gradient.
    public int centerColorTemperature; //Center color of a color temperature color gradient; is only used when colorGradientCount is 3.
    public int endColorTemperature; //End color of a color temperature color gradient.
}