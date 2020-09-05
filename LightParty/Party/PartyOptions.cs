using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace LightParty.Party
{
    class PartyOptions
    {
        #region General
        public static float randomInterval = 3; //Interval in seconds, in which the random color and/or the random brightness of the selected lights is changed.
        #endregion
        #region Brigthness

        //Brigthness
        public static int brightnessOptionIndex = 0; //Index of brightness options.
        //0 = change with microphone input, 1 = change randomly, 2 = do not change

        //Input
        public static double minSoundLevel = 45; //The minimum soundlevel, from which the brightness is changend.
        public static double maxSoundLevel = 85; //The maximum soundlevel, from which the brightness is changend.
        public static bool startWithZeroInRange = true; //Whether or not the brightness starts with zero at the minimum sound level. If deactivated, it will start at the minimum itself.

        //Random
        public static int minRandomBrightness = 35; //Minimum of the random brightness.
        public static int maxRandomBrightness = 100; //Maximum of the random brightness.

        #endregion
        #region Color

        public static int colorOptionIndex = 1; //Index of color options.
        //0 = change with microphone input, 1 = change on input difference, 2 = change randomly, 3 = do not change

        //Color gradient
        public static int colorGradientCount = 2; //Count of the selected colors in the color grandient.
        public static bool useRGBColor = true; //Whether or not the RGB color spectrum should be used. If deactived, only color temperatures are allowed.

        public static Color startColor = Color.FromArgb(255, 0, 255, 255); //Start color of the RGB color spectrum color gradient.
        public static Color centerColor = Color.FromArgb(255, 0, 255, 0); //Center color of the RGB color spectrum color gradient; is only used when colorGradientCount is 3.
        public static Color endColor = Color.FromArgb(255, 255, 255, 0); //End color of the RGB color spectrum color gradient.

        public static int startColorTemperature = 154; //Start color of the color temperature color gradient.
        public static int centerColorTemperature = 200; //Center color of the color temperature color gradient; is only used when colorGradientCount is 3.
        public static int endColorTemperature = 500; //End color of the color temperature color gradient.

        /// <summary>
        /// Returns the data of the color gradients as a ColorGradientInformation Type
        /// </summary>
        /// <returns>Data of the color gradients as a ColorGradientInformation Type</returns>
        public static ColorGradientInformation GetColorGradientInformation()
        {
            return new ColorGradientInformation()
            {
                startColor = startColor,
                centerColor = centerColor,
                endColor = endColor,

                startColorTemperature = startColorTemperature,
                centerColorTemperature = centerColorTemperature,
                endColorTemperature = endColorTemperature
            };
        }

        //Input difference
        public static float colorDifferencePercent = 3.5f; //Minimum microphone input difference in percent, when a new color should be generated.

        //Random color
        public static bool changeColorCompletelyRandom = false; //Whether or not the generated colors should be completely random.

        #endregion
    }
}
