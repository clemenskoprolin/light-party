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
        public static PartyOption activePartyOption;
        private static List<PartyOption> partyOptionSaves = new List<PartyOption>()
        {
            new PartyOption
            {
                randomInterval = 3,

                brightnessOptionIndex = 0,
                minSoundLevel = 45,
                maxSoundLevel = 85,
                startWithZeroInRange = true,

                minRandomBrightness = 35,
                maxRandomBrightness = 100,

                colorOptionIndex = 1,

                colorGradientCount = 2,
                useRGBColor = true,
                startColor = Color.FromArgb(255, 0, 255, 255),
                centerColor = Color.FromArgb(255, 0, 255, 0),
                endColor = Color.FromArgb(255, 255, 255, 0),

                startColorTemperature = 154,
                centerColorTemperature = 200,
                endColorTemperature = 500
            },
        };

        public static void SetPartyOption(int partyOptionId)
        {
            activePartyOption = partyOptionSaves[partyOptionId];
        }

        public static ColorGradientInformation GetColorGradientInformation()
        {
            return activePartyOption.GetColorGradientInformation();
        }
    }

    public class PartyOption
    {
        #region General
        public float randomInterval; //Interval in seconds, in which the random color and/or the random brightness of the selected lights is changed.
        #endregion
        #region Brigthness

        //Brigthness
        public int brightnessOptionIndex; //Index of brightness options.
        //0 = change with microphone input, 1 = change randomly, 2 = do not change

        //Input
        public double minSoundLevel; //The minimum soundlevel, from which the brightness is changend.
        public double maxSoundLevel; //The maximum soundlevel, from which the brightness is changend.
        public bool startWithZeroInRange = true; //Whether or not the brightness starts with zero at the minimum sound level. If deactivated, it will start at the minimum itself.

        //Random
        public int minRandomBrightness; //Minimum of the random brightness.
        public int maxRandomBrightness; //Maximum of the random brightness.

        #endregion
        #region Color

        public int colorOptionIndex; //Index of color options.
        //0 = change with microphone input, 1 = change on input difference, 2 = change randomly, 3 = do not change

        //Color gradient
        public int colorGradientCount; //Count of the selected colors in the color grandient.
        public bool useRGBColor = true; //Whether or not the RGB color spectrum should be used. If deactived, only color temperatures are allowed.

        public Color startColor; //Start color of the RGB color spectrum color gradient.
        public Color centerColor; //Center color of the RGB color spectrum color gradient; is only used when colorGradientCount is 3.
        public Color endColor; //End color of the RGB color spectrum color gradient.

        public int startColorTemperature; //Start color of the color temperature color gradient.
        public int centerColorTemperature; //Center color of the color temperature color gradient; is only used when colorGradientCount is 3.
        public int endColorTemperature; //End color of the color temperature color gradient.

        /// <summary>
        /// Returns the data of the color gradients as a ColorGradientInformation Type
        /// </summary>
        /// <returns>Data of the color gradients as a ColorGradientInformation Type</returns>
        public ColorGradientInformation GetColorGradientInformation()
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
        public float colorDifferencePercent = 3.5f; //Minimum microphone input difference in percent, when a new color should be generated.

        //Random color
        public bool changeColorCompletelyRandom = false; //Whether or not the generated colors should be completely random.

        #endregion
    }
}
