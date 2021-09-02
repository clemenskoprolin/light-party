﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI;
using Q42.HueApi;
using Q42.HueApi.ColorConverters.Original;
using LightParty.Services;
using LightParty.Connection;

namespace LightParty.LightController
{
    /// <summary>
    /// This class contains all the methods that get information about lights.
    /// </summary>
    class LightInformation
    {
        /// <summary>
        /// Gets the brightness of a light in percent.
        /// </summary>
        /// <param name="light">The light with which the method works</param>
        /// <returns>The brightness of the given light in percent</returns>
        public static int GetBrightnessInPercent(Light light)
        {
            int brightness = Convert.ToInt32((light.State.Brightness / 254f) * 100f);
            return brightness;
        }

        /// <summary>
        /// Return whether or not the light does only support the temperature color specturm.
        /// </summary>
        /// <param name="light">The light with which the method works</param>
        /// <returns>Whether or not the light does only support the color temperature specturm</returns>
        public static bool IsTemperatureType(Light light)
        {
            bool isTemperature = false;

            switch (light.Type)
            {
                case "Color temperature light":
                    isTemperature = true;
                    break;
            }

            return isTemperature;
        }

        /// <summary>
        /// Checks if at least one of the used lights is not a light which supports the rgb color spectrum.
        /// </summary>
        /// <returns>Whether or not at least one of the used lights is not a light which supports the RGB color spectrum.</returns>
        public static bool IsInRGBMode()
        {
            bool rgb = true;

            foreach (Light light in BridgeInformation.lights)
            {
                if (BridgeInformation.usedLights.Contains(light.Id))
                {
                    if (IsTemperatureType(light))
                        rgb = false;
                }
            }

            return rgb;
        }

        /// <summary>
        /// Checks if there are both lights which only support color temperature and lights which support the rgb color spectrum in usedLights.
        /// </summary>
        /// <returns>Whether or not there are both lights which only support color temperature and lights which support the rgb color spectrum in usedLights.</returns>
        public static bool IsInMixedColorSpectrumsMode()
        {
            bool rgb = false;
            bool temperature = false;

            foreach (Light light in BridgeInformation.lights)
            {
                if (BridgeInformation.usedLights.Contains(light.Id))
                {
                    if (IsTemperatureType(light))
                        temperature = true;
                    else
                        rgb = true;
                }
            }

            return rgb && temperature;
        }

        /// <summary>
        /// Gets an array of the used lights split in two lists, the first containing RGB lights and the second containing color temperature lights.
        /// </summary>
        /// <returns>An array of the used lights split in two lists, the first containing RGB lights and the second containing color temperature lights.</returns>
        public static List<string>[] GetUsedLightsSplitInSpectrums()
        {
            List<string> usedRGBlights = new List<string>();
            List<string> usedTermperatureLights = new List<string>();

            foreach (Light light in BridgeInformation.lights)
            {
                if (BridgeInformation.usedLights.Contains(light.Id))
                {
                    if (IsTemperatureType(light))
                        usedTermperatureLights.Add(light.Id);
                    else
                        usedRGBlights.Add(light.Id);
                }
            }

            return new List<string>[] { usedRGBlights, usedTermperatureLights };
        }

        /// <summary>
        /// Gets the color of a light.
        /// </summary>
        /// <param name="light">The light with which the method works</param>
        /// <returns>The color of a light.</returns>
        public static Color GetLightColor(Light light)
        {
            if (!IsTemperatureType(light))
            {
                Color color = ColorAssistant.ConvertRGBColorToColor(light.State.ToRGBColor());
                return color;
            }
            else
            {
                Color color = ColorAssistant.ConvertColorTemperatureToColor((int)light.State.ColorTemperature);
                return color;
            }
        }

        /// <summary>
        /// Returns if all of the used lights are turned on or not.
        /// </summary>
        /// <returns>If all of the used lights are turned on or not</returns>
        public static bool CheckIfTurnedOn()
        {
            foreach (Light light in BridgeInformation.lights)
            {
                if (BridgeInformation.usedLights.Contains(light.Id))
                {
                    if (!light.State.On)
                        return false;
                }
            }

            return true;
        }
    }
}
