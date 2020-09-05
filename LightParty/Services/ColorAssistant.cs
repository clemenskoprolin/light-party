using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.Original;
using System.Diagnostics;
using System.Globalization;

namespace LightParty.Services
{
    class ColorAssistant
    {
        private static Color rgbTemperaturStart = Color.FromArgb(255, 245, 253, 255);
        private static Color rgbTemperaturCenter = Color.FromArgb(255, 255, 218, 143);
        private static Color rgbTemperaturStop = Color.FromArgb(255, 255, 177, 65);

        /// <summary>
        /// Calculates the color of a between two color at a given point.
        /// </summary>
        /// <param name="startColor">The start color</param>
        /// <param name="endColor">The end color</param>
        /// <param name="step">The point as a float from 0 to 1 at which the color is calculated</param>
        /// <returns>The result as a Windows.UI.Color</returns>
        public static Color GetStepBetweenTwoColors(Color startColor, Color endColor, float step)
        {
            int r = Math.Abs(startColor.R + (int)((endColor.R - startColor.R) * step));
            int g = Math.Abs(startColor.G + (int)((endColor.G - startColor.G) * step));
            int b = Math.Abs(startColor.B + (int)((endColor.B - startColor.B) * step));

            r = r > 255 ? 255 : r;
            g = g > 255 ? 255 : g;
            b = b > 255 ? 255 : b;

            return Color.FromArgb(255, Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b));
        }

        /// <summary>
        /// Calculates the color of a between three color at a given point.
        /// </summary>
        /// <param name="startColor">The start color</param>
        /// <param name="centerColor">The center color</param>
        /// <param name="endColor">The end color</param>
        /// <param name="step">The point as a float from 0 to 1 at which the color is calculated</param>
        /// <returns>The result as a Windows.UI.Color</returns>
        public static Color GetStepBetweenThreeColors(Color startColor, Color centerColor, Color endColor, float step)
        {
            if (step < 0.5f)
                return GetStepBetweenTwoColors(startColor, centerColor, step * 2);
            else
                return GetStepBetweenTwoColors(centerColor, endColor, (step - 0.5f) * 2);
        }

        public static int GetStepBetweenTwoColorTemperatures(int startTemperature, int stopTemperature, float step)
        {
            int newTemperature = Convert.ToInt32(startTemperature + (stopTemperature - startTemperature) * step);
            return newTemperature;
        }

        public static int GetStepBetweenThreeColorTemperatures(int startTemperature, int centerTemperature, int stopTemperature, float step)
        {
            if (step < 0.5f)
                return GetStepBetweenTwoColorTemperatures(startTemperature, centerTemperature, step * 2);
            else
                return GetStepBetweenTwoColorTemperatures(centerTemperature, stopTemperature, step * 2);
        }

        public static Color ConvertRGBColorToColor(RGBColor inputRGBColor)
        {
            string hexCode = inputRGBColor.ToHex();

            Color color = new Color
            {
                A = 255,
                R = byte.Parse(hexCode.Substring(0, 2), NumberStyles.HexNumber),
                G = byte.Parse(hexCode.Substring(2, 2), NumberStyles.HexNumber),
                B = byte.Parse(hexCode.Substring(4, 2), NumberStyles.HexNumber),
            };

            return color;
        }

        public static Color ConvertColorTemperatureToColor(int temperatur)
        {
            //https://academo.org/demos/colour-temperature-relationship/ - 
            /* Philips Hue color temperature spectrum:
             * 153 Mireds - 500 Mireds*/

            float step = (temperatur - 153f) / (500f - 153f);
            return GetStepBetweenThreeColors(rgbTemperaturStart, rgbTemperaturCenter, rgbTemperaturStop, step);
        }

        public static int TryToConvertColorToColorTemperatur(Color inputColor)
        {
            float bestReferenceStep = -1;
            float bestReferenceDifference = 100;

            for (float i = 0; i < 1; i += 0.02f)
            {
                Color currentReference = GetStepBetweenThreeColors(rgbTemperaturStart, rgbTemperaturCenter, rgbTemperaturStop, i);
                float difference = CompareTwoColors(currentReference, inputColor);

                if (difference < bestReferenceDifference)
                {
                    bestReferenceDifference = difference;
                    bestReferenceStep = i;
                }
            }

            if (bestReferenceStep != -1 && bestReferenceDifference <= 10)
            {
                return Convert.ToInt32(bestReferenceStep * (500 - 153) + 153);
            }

            return -1;
        }

        private static float CompareTwoColors(Color color1, Color color2)
        {
            float grayColor1 = 0.11f * color1.B + 0.59f * color1.G + 0.30f * color1.R;
            float grayColor2 = 0.11f * color2.B + 0.59f * color2.G + 0.30f * color2.R;

            float difference = (grayColor1 - grayColor2) * 100 / 255;

            return Math.Abs(difference);
        }

        /// <summary>
        /// Gets a random color in the RGB color spectrum.
        /// </summary>
        /// <returns>A color as a RGBColor Type</returns>
        public static RGBColor GetRandomRGBColor()
        {
            Random random = new Random();
            return new RGBColor { R = random.Next(257), B = random.Next(257), G = random.Next(257)  };
        }

        /// <summary>
        /// Returns a random color termperature.
        /// </summary>
        /// <returns>A color temperature as integer from 153 to 500</returns>
        public static int GetRandomColorTemperature()
        {
            Random random = new Random();
            return 153 + random.Next(348);
        }
    }
}
