using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightParty.Connection;
using LightParty.LightController;

namespace LightParty.Party
{
    class LightProcessingRandom
    {
        private static int updateInterval = 3000;
        public static bool isUpdating = false;

        public static void SetUpdateInterval(string intervalInSecondsString)
        {
            if (float.TryParse(intervalInSecondsString, out float intervalInSeconds))
            {
                if (intervalInSeconds >= 0.2f)
                {
                    updateInterval = Convert.ToInt32(intervalInSeconds * 1000);

                    PartyUIUpdater.NewRandomInterval(intervalInSeconds);
                    PartyOptions.activePartyOption.randomInterval = intervalInSeconds;
                }
            }

            PartyUIUpdater.UpdateRandomIntervalTextBoxes();
        }

        public static void StartUpdates()
        {
            isUpdating = true;
            UpdateInterval();
        }

        public static void StopUpdates()
        {
            isUpdating = false;
        }

        private static async void UpdateInterval()
        {
            while (isUpdating)
            {
                await Task.Delay(updateInterval);
                UpdateLights();
            }
        }

        private static void UpdateLights()
        {
            int? newBrightness = null;
            ColorInformation colorInformation = new ColorInformation();

            switch (PartyOptions.activePartyOption.brightnessOptionIndex)
            {
                case 1:
                    newBrightness = SetRandomBrightness();
                    break;
            }

            switch (PartyOptions.activePartyOption.colorOptionIndex)
            {
                case 2:
                    colorInformation = LightProcessingColor.SetRandomColorFromUIInput();
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
        }

        private static int? SetRandomBrightness()
        {
            Random random = new Random();
            int randomBrightness = random.Next(PartyOptions.activePartyOption.minRandomBrightness, PartyOptions.activePartyOption.maxRandomBrightness);
            BasicLightController.SetBrightnessInCommonCommand(randomBrightness);

            return randomBrightness;
        }
    }
}
