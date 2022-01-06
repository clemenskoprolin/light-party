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
        private static int callCountMinMicrophone = 8; //Every callCountMin times the methode NewSoundLevel is called, the new sound level will be processed. THis only applys to an microphone input.
        private static int callCountMinLoopback = 1; //Every callCountMin times the methode NewSoundLevel is called, the new sound level will be processed. THis only applys to an Loopback input.
        private static double savedSoundLevel = 0; //Sound level of the last process. Is used to calculate the input difference.

        private static double targetMinAudioLevel = 100; //Current target of minAudioLevel. AutomaticRangeSelection changes its value and moves slowly minSoundLevel to it.
        private static double targetMaxAudioLevel = 0; //Current target of maxAudioLevel. AutomaticRangeSelection changes its value and moves slowly maxSoundLevel to it.
        private static double targetLifeSpan = 200; //Lifespan of the targets, before they will be replaced.
        private static double currentMinTargetLife = 0; //Counts, how often targetMinAudioLevel was used.
        private static double currentMaxTargetLife = 0; //Counts, how often targetMaxAudioLevel was used.

        /// <summary>
        /// Is called when a new sound level was calculated. Calls the necessary methods based on PartyOptions, sends the common command and updates the output display.
        /// </summary>
        /// <param name="soundLevel">The newly calculated sound level</param>
        public static void NewSoundLevel(double soundLevel)
        {
            //Íf automaticRangeSelection is activated, minSoundLevel and maxSoundLevel will be changed according to AutomaticRangeSelection.
            if (PartyOptions.activePartyOption.automaticRangeSelection)
            {
                double[] result = AutomaticRangeSelection(soundLevel);
                if (result[0] != PartyOptions.activePartyOption.minSoundLevel || result[1] != PartyOptions.activePartyOption.maxSoundLevel)
                {
                    PartyOptions.activePartyOption.minSoundLevel = result[0];
                    PartyOptions.activePartyOption.maxSoundLevel = result[1];

                    PartyUIUpdater.NewAudioInputRange(result[0], result[1]);
                }
            }

            //Sets callCountMinMicrophone based on the number of used lights. E.g. 1 used light --> 8; 2 used lights --> 16; ...
            callCountMinMicrophone = BridgeInformation.usedLights.Count < 1 ? 8 : BridgeInformation.usedLights.Count * 8;
            //Sets callCountMinLoopback based on the number of used lights. E.g. 1 used light --> 1; 2 used lights --> 1; 3 used lights --> 1; 4 used lights --> 2; ...
            callCountMinLoopback = BridgeInformation.usedLights.Count < 1 ? 1 : (int)(BridgeInformation.usedLights.Count * 1.25f);

            if ((PartyOptions.activePartyOption.audioSource == 0 && callCount > callCountMinMicrophone) || (PartyOptions.activePartyOption.audioSource == 1 && callCount > callCountMinLoopback))
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

        /// <summary>
        /// Slowly moves minSoundLevel and maxSoundLevel the minimum and maximum of the recent audio input. The duration is influenced by targetLifeSpan. 
        /// </summary>
        /// <param name="newSoundLevel">The current audio level</param>
        /// <returns>minSoundLevel and maxSoundLevelk in an array of doubles</returns>
        private static double[] AutomaticRangeSelection(double newSoundLevel)
        {
            double[] range = { PartyOptions.activePartyOption.minSoundLevel, PartyOptions.activePartyOption.maxSoundLevel };
            if (newSoundLevel > 10)
            {
                currentMinTargetLife++;
                currentMaxTargetLife++;
                if (newSoundLevel < PartyOptions.activePartyOption.minSoundLevel || currentMinTargetLife > targetLifeSpan)
                {
                    targetMinAudioLevel = newSoundLevel;
                    currentMinTargetLife = 0;
                }
                if (newSoundLevel > PartyOptions.activePartyOption.maxSoundLevel || currentMaxTargetLife > targetLifeSpan)
                {
                    targetMaxAudioLevel = newSoundLevel + 5;
                    currentMaxTargetLife = 0;
                }

                range[0] += (targetMinAudioLevel - range[0]) * 0.01f;
                range[1] += (targetMaxAudioLevel - range[1]) * 0.01f;
            }

            return range;
        }

        private static int ProcessSoundLevelBrightness(double soundLevel)
        {
            double brightnessDouble = 0;
            int brightness = 0;

            if (!(soundLevel < PartyOptions.activePartyOption.minSoundLevel && PartyOptions.activePartyOption.startWithZeroInRange))
            {
                soundLevel = soundLevel > PartyOptions.activePartyOption.maxSoundLevel ? PartyOptions.activePartyOption.maxSoundLevel : soundLevel;

                brightnessDouble = (soundLevel - PartyOptions.activePartyOption.minSoundLevel) / (PartyOptions.activePartyOption.maxSoundLevel - PartyOptions.activePartyOption.minSoundLevel) * 100;
                if (!PartyOptions.activePartyOption.startWithZeroInRange)
                    brightnessDouble = brightnessDouble < PartyOptions.activePartyOption.minSoundLevel ? PartyOptions.activePartyOption.minSoundLevel : brightnessDouble;
            }

            try
            {
                brightness = Convert.ToInt32(brightnessDouble);
                BasicLightController.SetBrightnessInCommonCommand(brightness);
            }
            catch
            {
                brightness = 0;
            }

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
