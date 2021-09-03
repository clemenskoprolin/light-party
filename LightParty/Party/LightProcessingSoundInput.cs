using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using LightParty.Connection;
using LightParty.LightController;
using LightParty.Services;
using Q42.HueApi.ColorConverters;

namespace LightParty.Party
{
    /// <summary>
    /// This class contains the methods that are used to calculate the new brightness and color of the lights based on a sound level input.
    /// </summary>
    class LightProcessingSoundInput
    {
        private static int callCount = 0; //Is increased every time NewSoundLevel is called;
        private static int callCountMin = 8; //Every callCountMin times the methode NewSoundLevel is called, the new sound level will be processed.
        private static double savedSoundLevel = 0; //Sound level of the last process. Is used to calculate the input difference.

        /// <summary>
        /// Is called when a new sound level was calculated. Calls the necessary methods based on PartyOptions, sends the common command and updates the output display.
        /// </summary>
        /// <param name="soundLevel">The newly calculated sound level</param>
        public static void NewSoundLevel(double soundLevel)
        {
            //Sets callCountMin based on the number of used lights. E.g. 1 used light --> 8; 2 used lights --> 16; ...
            callCountMin = BridgeInformation.usedLights.Count < 1 ? 8 : BridgeInformation.usedLights.Count * 8;

            if (callCount > callCountMin)
            {
                int? newBrightness = null;
                ColorInformation colorInformation = new ColorInformation();

                switch (PartyOptions.activePartyOption.brightnessOptionIndex)
                {
                    case 0:
                        newBrightness = ProcessSoundLevelBrightness(soundLevel);
                        break;
                }

                switch (PartyOptions.activePartyOption.colorOptionIndex)
                {
                    case 0:
                        colorInformation = LightProcessingColor.SetColorGradienStep((float)soundLevel / 100f);
                        break;

                    case 1:
                        colorInformation = ProcessInputDifferenceColor(soundLevel);
                        break;
                }

                if (BridgeInformation.usedLights.Count > 0)
                {
                    if (!PartyOptions.useMixedColorSpectrums)
                        BasicLightController.SendCommonCommond();
                    else
                        BasicLightController.SendCommonCommond(true);
                }

                if (newBrightness != null || colorInformation.rgbColor != null || colorInformation.colorTemperature != null)
                    PartyUIUpdater.UpdateOutputDisplay(newBrightness, colorInformation.rgbColor, colorInformation.colorTemperature);

                callCount = 0;
            }

            callCount++;
        }

        private static int ProcessSoundLevelBrightness(double soundLevel)
        {
            double brightnessDouble = 0;

            if (!(soundLevel < PartyOptions.activePartyOption.minSoundLevel && PartyOptions.activePartyOption.startWithZeroInRange))
            {
                soundLevel = soundLevel > PartyOptions.activePartyOption.maxSoundLevel ? PartyOptions.activePartyOption.maxSoundLevel : soundLevel;

                brightnessDouble = (soundLevel - PartyOptions.activePartyOption.minSoundLevel) / (PartyOptions.activePartyOption.maxSoundLevel - PartyOptions.activePartyOption.minSoundLevel) * 100;
                if (!PartyOptions.activePartyOption.startWithZeroInRange)
                    brightnessDouble = brightnessDouble < PartyOptions.activePartyOption.minSoundLevel ? PartyOptions.activePartyOption.minSoundLevel : brightnessDouble;
            }

            int brightness = Convert.ToInt32(brightnessDouble);
            BasicLightController.SetBrightnessInCommonCommand(brightness);

            return brightness;
        }

        private static ColorInformation ProcessInputDifferenceColor(double soundLevel)
        {
            ColorInformation colorInformation = new ColorInformation();

            double inputDifference = soundLevel - savedSoundLevel;

            if (inputDifference > PartyOptions.activePartyOption.colorDifferencePercent)
            {
                colorInformation = LightProcessingColor.SetRandomColorFromUIInput();
            }

            PartyUIUpdater.NewInputDifference(inputDifference);
            savedSoundLevel = soundLevel;

            return colorInformation;
        }
    }
}
