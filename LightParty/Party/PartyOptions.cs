﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace LightParty.Party
{
    /// <summary>
    /// This class contains all templates and options that are used by the PartyMode
    /// </summary>
    class PartyOptions
    {
        public static bool useRGBColor = true; //Whether or not the RGB color spectrum should be used. If deactived, only color temperatures are allowed.
        public static PartyOption activePartyOption; //The actively used instance of PartyOptions. All changes that the user makes are saved here.
        private static readonly List<PartyOption> partyOptionSaves = new List<PartyOption>() //This list contains the saves (templates) of PartyOption. They can be selected by the user in PartyControlSimple.
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
                startColor = Color.FromArgb(255, 0, 255, 255),
                centerColor = Color.FromArgb(255, 0, 255, 0),
                endColor = Color.FromArgb(255, 255, 255, 0),

                startColorTemperature = 154,
                centerColorTemperature = 200,
                endColorTemperature = 500,

                colorDifferencePercent = 2.5f,
                changeColorCompletelyRandom = true,
            },
            new PartyOption
            {
                randomInterval = 3,

                brightnessOptionIndex = 1,
                minSoundLevel = 45,
                maxSoundLevel = 85,
                startWithZeroInRange = true,

                minRandomBrightness = 35,
                maxRandomBrightness = 100,

                colorOptionIndex = 2,

                colorGradientCount = 2,
                startColor = Color.FromArgb(255, 0, 255, 255),
                centerColor = Color.FromArgb(255, 0, 255, 0),
                endColor = Color.FromArgb(255, 255, 255, 0),

                startColorTemperature = 154,
                centerColorTemperature = 200,
                endColorTemperature = 500,

                colorDifferencePercent = 2.5f,
                changeColorCompletelyRandom = true,
            },
        };

        /// <summary>
        /// Replaces activePartyOption with a save whose ID is given.
        /// </summary>
        /// <param name="partyOptionId">The ID of the save. It's the index of it in partyOptionSaves</param>
        public static void SetPartyOption(int partyOptionId)
        {
            activePartyOption = partyOptionSaves[partyOptionId];
        }

        /// <summary>
        /// Compares activePartyOption with partyOptionSaves. Returns the index of the save, if all variables are equal. When not, -1 is returned.
        /// </summary>
        /// <returns>The index of the save, if all variables are equal. When not, -1 is returned</returns>
        public static int CompareCurrentWithSaves()
        {
            for (int i = 0; i < partyOptionSaves.Count; i++)
            {
                if (activePartyOption == partyOptionSaves[i])
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns the data of the color gradients from the active PartyOption as a ColorGradientInformation Type
        /// </summary>
        /// <returns>Data of the color gradients from the active PartyOption as a ColorGradientInformation Type</returns>
        public static ColorGradientInformation GetColorGradientInformation()
        {
            return activePartyOption.GetColorGradientInformation();
        }
    }

    /// <summary>
    /// This class contains all variables that can be changed in the Party Mode.
    /// </summary>
    public struct PartyOption
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
        public bool startWithZeroInRange; //Whether or not the brightness starts with zero at the minimum sound level. If deactivated, it will start at the minimum itself.

        //Random
        public int minRandomBrightness; //Minimum of the random brightness.
        public int maxRandomBrightness; //Maximum of the random brightness.

        #endregion
        #region Color

        public int colorOptionIndex; //Index of color options.
        //0 = change with microphone input, 1 = change on input difference, 2 = change randomly, 3 = do not change

        //Color gradient
        public int colorGradientCount; //Count of the selected colors in the color grandient.

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
        public float colorDifferencePercent; //Minimum microphone input difference in percent, when a new color should be generated.

        //Random color
        public bool changeColorCompletelyRandom; //Whether or not the generated colors should be completely random.

        #endregion

        /// <summary>
        /// Compares all fields (variables) in a PartyOption to another except the value of randomInterval.
        /// </summary>
        /// <param name="a">The first PartyOption</param>
        /// <param name="b">The second PartyOption</param>
        /// <returns>Whether or not all fields except randomInterval are equal</returns>
        public static bool operator ==(PartyOption a, PartyOption b)
        {
            FieldInfo[] aFields = a.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            FieldInfo[] bFields = b.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            for (int i = 0; i < aFields.Length; i++)
            {
                if (aFields[i].Name != "randomInterval")
                {
                    if (!aFields[i].GetValue(a).Equals(bFields[i].GetValue(b)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Compares all fields (variables) in a PartyOption to another except the value of randomInterval.
        /// </summary>
        /// <param name="a">The first PartyOption</param>
        /// <param name="b">The second PartyOption</param>
        /// <returns>Whether or not all fields except randomInterval are NOT equal</returns>
        public static bool operator !=(PartyOption a, PartyOption b)
        {
            FieldInfo[] aFields = a.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            FieldInfo[] bFields = b.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            for (int i = 0; i < aFields.Length; i++)
            {
                if (aFields[i].Name != "randomInterval")
                {
                    if (aFields[i].GetValue(a).Equals(bFields[i].GetValue(b)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
