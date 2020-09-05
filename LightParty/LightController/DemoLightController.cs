using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightParty.Connection;
using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.Original;

namespace LightParty.LightController
{
    /// <summary>
    /// This class contains all methods that create and manage demo lights which are used in the demo mode.
    /// </summary>
    class DemoLightController
    {
        public static RGBColor commonRGBColor;  //Common RGB color, which is needed to set the RGB color of a common command to demo lights.

        /// <summary>
        /// Creates two demo RGB lights and two demo color temperature lights and adds them to BridgeInformation.lights.
        /// </summary>
        public static void CreatesDemoLights()
        {
            Light rgbLight1 = new Light
            {
                Name = "RGB Light 1",
                Id = "1",
                Type = "RGB light",

                State = new State
                {
                    On = true,

                    Brightness = 254,
                    Saturation = 100,
                    ColorCoordinates = new double[] { 100, 100 }
                }
            };

            Light rgbLight2 = new Light
            {
                Name = "RGB light 2",
                Id = "2",
                Type = "RGB light",

                State = new State
                {
                    On = false,

                    Brightness = 200,
                    Saturation = 100,
                    ColorCoordinates = new double[] { 0, 0 }
                }
            };

            Light temperatureLight1 = new Light
            {
                Name = "temperature light 1",
                Id = "3",
                Type = "Color temperature light",

                State = new State
                {
                    On = true,

                    Brightness = 254,
                    ColorTemperature = 153
                }
            };
            
            Light temperatureLight2 = new Light
            {
                Name = "temperature light 2",
                Id = "4",
                Type = "Color temperature light",

                State = new State
                {
                    On = true,

                    Brightness = 50,
                    ColorTemperature = 500
                }
            };

            BridgeInformation.lights = new Light[] { rgbLight1, rgbLight2, temperatureLight1, temperatureLight2 };
        }

        /// <summary>
        /// Turns the used demo lights on.
        /// </summary>
        public static void TurnOn()
        {
            foreach (Light light in BridgeInformation.lights)
            {
                if (BridgeInformation.usedLights.Contains(light.Id))
                    light.State.On = true;
            }
        }

        /// <summary>
        /// Turns the used demo lights off.
        /// </summary>
        public static void TurnOff()
        {
            foreach (Light light in BridgeInformation.lights)
            {
                if (BridgeInformation.usedLights.Contains(light.Id))
                    light.State.On = false;
            }
        }

        /// <summary>
        /// Sets the brightness of the used demo lights.
        /// </summary>
        /// <param name="newBrightnessPercent">New brightness percent from 0 to 100</param>
        public static void SetBrightness(int newBrightnessPercent)
        {
            foreach (Light light in BridgeInformation.lights)
            {
                if (BridgeInformation.usedLights.Contains(light.Id))
                    light.State.Brightness = (byte)((float)newBrightnessPercent / 100 * 254); ;
            }
        }

        /// <summary>
        /// Sets the RGB color of the used demo lights.
        /// </summary>
        /// <param name="rgbColor">New color in the color type of the API</param>
        public static void SetRGBColor(RGBColor rgbColor)
        {
            LightCommand lightCommand = new LightCommand();
            lightCommand.SetColor(rgbColor);

            foreach (Light light in BridgeInformation.lights)
            {
                if (BridgeInformation.usedLights.Contains(light.Id))
                {
                    light.State.ColorCoordinates = lightCommand.ColorCoordinates;
                    light.State.Saturation = lightCommand.Saturation;
                }
            }
        }

        /// <summary>
        /// Sets the color temperaute of the used demo lights.
        /// </summary>
        /// <param name="newColorTemperature">New color temperature in mireds from 153 to 500</param>
        public static void SetColorTemperature(int newColorTemperature)
        {
            foreach (Light light in BridgeInformation.lights)
            {
                if (BridgeInformation.usedLights.Contains(light.Id))    
                    light.State.ColorTemperature = newColorTemperature;
            }
        }

        /// <summary>
        /// Sets the properties of a lightCommand to the used demo lights.
        /// </summary>
        /// <param name="lightCommand">Light command from which the properties are set</param>
        public static void SetPropertiesOfCommand(LightCommand lightCommand)
        {
            if (lightCommand.On != null)
            {
                if ((bool)lightCommand.On)
                    TurnOn();
                else
                    TurnOff();
            }
            
            if (lightCommand.Brightness != null)
            {
                SetBrightness((int)lightCommand.Brightness);
            }

            if (lightCommand.ColorCoordinates != null)
            {
                SetRGBColor(commonRGBColor);
            }

            if (lightCommand.ColorTemperature != null)
            {
                SetColorTemperature((int)lightCommand.ColorTemperature);
            }
        }
    }
}
