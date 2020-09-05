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
    class LightProcessingSoundInput
    {
        private static int callCount = 0;
        private static double savedSoundLevel = 0;

        public static void NewSoundLevel(double soundLevel)
        {
            if (callCount > 8)
            {
                int? newBrightness = null;
                ColorInformation colorInformation = new ColorInformation();

                switch (PartyOptions.brightnessOptionIndex)
                {
                    case 0:
                        newBrightness = ProcessSoundLevelBrightness(soundLevel);
                        break;
                }

                switch (PartyOptions.colorOptionIndex)
                {
                    case 0:
                        colorInformation = LightProcessingColor.SetColorGradienStep((float)soundLevel / 100f);
                        break;

                    case 1:
                        colorInformation = ProcessInputDifferenceColor(soundLevel);
                        break;
                }

                if (BridgeInformation.usedLights.Count > 0)
                    BasicLightController.SendCommonCommond();

                if (newBrightness != null || colorInformation.rgbColor != null || colorInformation.colorTemperature != null)
                    PartyUIUpdater.UpdateOutputDisplay(newBrightness, colorInformation.rgbColor, colorInformation.colorTemperature);

                callCount = 0;
            }

            callCount++;
        }

        private static int ProcessSoundLevelBrightness(double soundLevel)
        {
            double brightnessDouble = 0;

            if (!(soundLevel < PartyOptions.minSoundLevel && PartyOptions.startWithZeroInRange))
            {
                soundLevel = soundLevel > PartyOptions.maxSoundLevel ? PartyOptions.maxSoundLevel : soundLevel;

                brightnessDouble = (soundLevel - PartyOptions.minSoundLevel) / (PartyOptions.maxSoundLevel - PartyOptions.minSoundLevel) * 100;
                if (!PartyOptions.startWithZeroInRange)
                    brightnessDouble = brightnessDouble < PartyOptions.minSoundLevel ? PartyOptions.minSoundLevel : brightnessDouble;
            }

            int brightness = Convert.ToInt32(brightnessDouble);
            BasicLightController.SetBrightnessInCommonCommand(brightness);

            return brightness;
        }

        private static ColorInformation ProcessInputDifferenceColor(double soundLevel)
        {
            ColorInformation colorInformation = new ColorInformation();

            double inputDifference = soundLevel - savedSoundLevel;

            if (inputDifference > PartyOptions.colorDifferencePercent)
            {
                colorInformation = LightProcessingColor.SetRandomColorFromUIInput();
            }

            PartyUIUpdater.NewInputDifference(inputDifference);
            savedSoundLevel = soundLevel;

            return colorInformation;
        }
    }
}
