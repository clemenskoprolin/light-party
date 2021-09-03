using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;
using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.Original;
using LightParty.Connection;

namespace LightParty.LightController
{
    /// <summary>
    /// This class contains all methods that send commands to lights or demtermine if a light is used or not.
    /// </summary>
    class BasicLightController
    {
        public static bool canControl = false; //Determines, whether  a command can be send or not.
        private static LightCommand commonLightCommand = new LightCommand(); //Used to store a light command to which multiple attitudes will be added.
        private static LightCommand commonLightCommandRGB = new LightCommand(); //In some cases having a separate light command for rgb lights is necessary.
        private static LightCommand commonLightCommandTemperature = new LightCommand(); //In some cases having a separate light command for color temperature lights is necessary^.
        private static readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(); //Is used to cancel GetAllLights after 3.5 seconds.

        #region light selection

        /// <summary>
        /// Trys to get all lights and stores them in BridgeInformation.lights. Cancels the operation after 3.5 seconds.
        /// </summary>
        /// /// <returns>Whether or not the operation was successful.</returns>
        public static async Task<bool> GetAllLights()
        {
            if (BridgeInformation.demoMode)
            {
                if (BridgeInformation.lights.Length == 0)
                    DemoLightController.CreatesDemoLights();
                return true;
            }

            try
            {
                cancellationTokenSource.CancelAfter(3500);
                var result = await BridgeInformation.client.GetBridgeAsync();
                BridgeInformation.lights = result.Lights.ToArray();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void ClearAllLights()
        {
            BridgeInformation.lights = new Light[] { };
        }

        /// <summary>
        /// Adds or removes a light from the list of used lights.
        /// </summary>
        /// <returns>Whether the light is in the array of the used lights or not.</returns>
        public static bool ChangeLightSelection(string id)
        {
            if (!BridgeInformation.usedLights.Contains(id))
            {
                BridgeInformation.usedLights.Add(id);
                return true;
            }
            else
            {
                if (BridgeInformation.mainLightTarget.Id == id)
                {
                    BridgeInformation.mainLightTarget = null;
                }
                BridgeInformation.usedLights.Remove(id.ToString());
                return false;
            }
        }

        #endregion
        #region light state

        /// <summary>
        /// Turns the used lights on.
        /// </summary>
        public static void TurnOn()
        {
            if (!canControl)
                return;

            if (BridgeInformation.demoMode)
            {
                DemoLightController.TurnOn();
                return;
            }

            LightCommand lightCommand = new LightCommand();
            lightCommand.TurnOn();

            BridgeInformation.client.SendCommandAsync(lightCommand, BridgeInformation.usedLights);
        }

        /// <summary>
        /// Turns the used lights off.
        /// </summary>
        public static void TurnOff()
        {
            if (!canControl)
                return;

            if (BridgeInformation.demoMode)
            {
                DemoLightController.TurnOff();
                return;
            }

            LightCommand lightCommand = new LightCommand();
            lightCommand.TurnOff();

            BridgeInformation.client.SendCommandAsync(lightCommand, BridgeInformation.usedLights);
        }

        #endregion
        #region brigthness 

        /// <summary>
        /// Sets the brightness of the used lights.
        /// </summary>
        /// <param name="newBrightnessPercent">New brightness percent from 0 to 100</param>
        public static void SetBrightness(int newBrightnessPercent)
        {
            if (!canControl)
                return;

            if (BridgeInformation.demoMode)
            {
                DemoLightController.SetBrightness(newBrightnessPercent);
                return;
            }

            //The API has minimum brightness of 0 and a maximum of 254.
            //The following line converts the brightness in percent to the brightness of API
            byte newBrightness = (byte)((float)newBrightnessPercent / 100 * 254); //e.g. 50% --> 127

            LightCommand lightCommand = new LightCommand();
            lightCommand.Brightness = newBrightness;

            BridgeInformation.client.SendCommandAsync(lightCommand, BridgeInformation.usedLights);
        }

        /// <summary>
        /// Sets the brightness of the common command.
        /// </summary>
        /// <param name="newBrightnessPercent">New brightness percent from 0 to 100</param>
        public static void SetBrightnessInCommonCommand(int newBrightnessPercent)
        {
            if (!canControl)
                return;

            //See above
            byte newBrightness = (byte)((float)newBrightnessPercent / 100 * 254);
            commonLightCommand.Brightness = newBrightness;
            commonLightCommandRGB.Brightness = newBrightness;
            commonLightCommandTemperature.Brightness = newBrightness;
        }

        #endregion
        #region color

        /// <summary>
        /// Sets the color of the used lights.
        /// </summary>
        /// <param name="rgbColor">New color in the color type of the API</param>
        public static void SetRGBColor(RGBColor rgbColor)
        {
            if (!canControl)
                return;

            if (BridgeInformation.demoMode)
            {
                DemoLightController.SetRGBColor(rgbColor);
                return;
            }

            LightCommand lightCommand = new LightCommand();
            lightCommand.SetColor(rgbColor);

            BridgeInformation.client.SendCommandAsync(lightCommand, BridgeInformation.usedLights);
        }

        /// <summary>
        /// Sets the color of the common command.
        /// </summary>
        /// <param name="rgbColor">New color in the color type of the API</param>
        public static void SetRGBColorInCommonCommand(RGBColor rgbColor)
        {
            if (!canControl)
                return;

            if (BridgeInformation.demoMode)
                DemoLightController.commonRGBColor = rgbColor;

            commonLightCommand.SetColor(rgbColor);
            commonLightCommandRGB.SetColor(rgbColor);
        }

        #endregion
        #region color temperature

        /// <summary>
        /// Sets the color temperature of the used lights.
        /// </summary>
        /// <param name="newColorTemperature">New color temperature in mireds from 153 to 500</param>
        public static void SetColorTemperature(int newColorTemperature)
        {
            if (!canControl)
                return;

            if (BridgeInformation.demoMode)
            {
                DemoLightController.SetColorTemperature(newColorTemperature);
                return;
            }

            LightCommand lightCommand = new LightCommand();
            lightCommand.SetColor(newColorTemperature);

            BridgeInformation.client.SendCommandAsync(lightCommand, BridgeInformation.usedLights);
        }

        /// <summary>
        /// Sets the color temperature of the common command.
        /// </summary>
        /// <param name="newColorTemperature">New color temperature in mireds from 153 to 500</param>
        public static void SetColorTemperatureInCommonCommand(int newColorTemperature)
        {
            if (!canControl)
                return;

            commonLightCommand.SetColor(newColorTemperature);
            commonLightCommandTemperature.SetColor(newColorTemperature);
        }

        #endregion
        #region common command

        /// <summary>
        /// Sends the common command to the used lights or separately the rgb and color temperature lights.
        /// </summary>
        /// <param name="splitSpectrums">If set to true, commonLightCommandRGB will be send to rgb lights and commonLightCommandTemperature will be send to color temperature lights</param>
        public static void SendCommonCommond(bool splitSpectrums = false)
        {
            if (BridgeInformation.demoMode)
            {
                DemoLightController.SetPropertiesOfCommand(commonLightCommand);
                return;
            }

            if (!splitSpectrums)
            {
                BridgeInformation.client.SendCommandAsync(commonLightCommand, BridgeInformation.usedLights);
            }
            else
            {
                List<string>[] splitUsedLights = LightInformation.GetUsedLightsSplitInSpectrums();
                BridgeInformation.client.SendCommandAsync(commonLightCommandRGB, splitUsedLights[0]);
                BridgeInformation.client.SendCommandAsync(commonLightCommandTemperature, splitUsedLights[1]);
            }
        }

        #endregion
    }
}
