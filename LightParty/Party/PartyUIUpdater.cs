using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Q42.HueApi.ColorConverters;
using LightParty.Services;
using LightParty.Pages.PartyMode.Advanced;

namespace LightParty.Party
{
    /// <summary>
    /// This class updates the visible UI elements of the Party Mode page.
    /// </summary>
    class PartyUIUpdater
    {
        static dynamic sliderPageOne; //Reference to the first class which contains a sound level slider.
        static dynamic sliderPageTwo; //Reference to the second class which contains a sound level slider.
        private static bool useSliderPageOne = false; //Whether or not the first reference is used.
        private static bool useSliderPageTwo = false; //Whether or not the second reference is used.

        private static int sliderUpdateCount = 0; //Used to determin, whether or not the sliders should be updated.

        static dynamic differencePage; //Reference to the class that contains a input difference text box.
        private static bool useDifferencePage = false; //Whether or not the reference is used.

        static dynamic intervalPageOne; //Reference to the first class which contains a random update interval text box.
        static dynamic intervalPageTwo; //Reference to the second class which contains a random update interval text box.
        private static bool useIntervalPageOne = false; //Whether or not the first reference is used.
        private static bool useIntervalPageTwo = false; //Whether or not the second reference is used.
        private static float randomInterval; //The current random update interval.

        private static PartyControlAdvanced partyControlAdvanced; //Reference to the class which contains the output display

        #region Sound level slider

        /// <summary>
        /// Sets the variables of the classes that contain the sound level slider that should be changed.
        /// </summary>
        /// <typeparam name="TypeOne">Type of the fist class</typeparam>
        /// <typeparam name="TypeTwo">Type of the second class></typeparam>
        /// <param name="newSliderPageOne">Reference to the first class</param>
        /// <param name="newSliderPageTwo">Reference to the second class</param>
        public static void GiveVariablesSlider<TypeOne, TypeTwo>(TypeOne newSliderPageOne, TypeTwo newSliderPageTwo)
        {
            useSliderPageOne = false;
            useSliderPageTwo = false;

            if (newSliderPageOne != null)
            {
                sliderPageOne = newSliderPageOne;
                useSliderPageOne = true;
            }

            if (newSliderPageTwo != null)
            {
                sliderPageTwo = newSliderPageTwo;
                useSliderPageTwo = true;
            }
        }

        /// <summary>
        /// Is called when a new sound level was calculated.
        /// </summary>
        /// <param name="soundLevel">The new sound level</param>
        public static void NewSoundLevel(double soundLevel)
        {
            sliderUpdateCount++;

            if (sliderUpdateCount > 3)
            {
                double value = Math.Round(soundLevel);
                SetMicrophoneInputSliders(value);

                sliderUpdateCount = 0;
            }
        }

        /// <summary>
        /// Sets, if visable, the microphone input sliders to a given value.
        /// </summary>
        private static async void SetMicrophoneInputSliders(double value)
        {
            //Runs the code in the UI thread.
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                if (useSliderPageOne)
                    sliderPageOne.SetMicrophoneInputSlider(value);

                if (useSliderPageTwo)
                    sliderPageTwo.SetMicrophoneInputSlider(value);
            });
        }

        /// <summary>
        /// Is called when the audio input range was changed.
        /// </summary>
        /// <param name="min">The new minimum</param>
        /// <param name="max">The new maximum</param>
        public static void NewAudioInputRange(double min, double max)
        {
            if (sliderUpdateCount == 0)
            {
                SetMicrophoneInputRangeSelectors(Math.Round(min), Math.Round(max));
            }
        }

        /// <summary>
        /// Sets, if visable, the microphone input range selectors to the given values.
        /// </summary>
        private static async void SetMicrophoneInputRangeSelectors(double min, double max)
        {
            //Runs the code in the UI thread.
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                if (useSliderPageTwo)
                    sliderPageTwo.SetMicrophoneInputRangeSelector(min, max);
            });
        }

        #endregion
        #region Input difference text

        /// <summary>
        /// Sets the variable of the class which contains the input difference text.
        /// </summary>
        /// <typeparam name="TypeOne">Type of the class</typeparam>
        /// <param name="newDifferencePage">Reference to the class</param>
        public static void GiveVariablesInputDifference<TypeOne>(TypeOne newDifferencePage)
        {
            differencePage = newDifferencePage;
            useDifferencePage = true;
        }

        /// <summary>
        /// Is called when a new input difference was calculated.
        /// </summary>
        /// <param name="soundLevel">The input difference</param>
        public static void NewInputDifference(double inputDifference)
        {
            double value = Math.Round(inputDifference, 1);
            value = value < 0 ? 0 : value;

            SetInputDifferenceElements(value);
        }

        /// <summary>
        /// Sets, if visable, the input difference text box to a given value.
        /// </summary>
        private async static void SetInputDifferenceElements(double value)
        {
            //Runs the code in the UI thread.
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                if (useDifferencePage)
                    differencePage.UpdateInputDifferenceText(value);
            });
        }

        #endregion
        #region Random update interval text boxes

        /// <summary>
        /// Sets the variables of the classes that contain the random update interval text boxes that should be changed.
        /// </summary>
        /// <typeparam name="TypeOne">Type of the fist class</typeparam>
        /// <typeparam name="TypeTwo">Type of the second class></typeparam>
        /// <param name="pageOne">Reference to the first class</param>
        /// <param name="pageTwo">Reference to the second class</param>
        public static void GiveVariablesInterval<TypeOne, TypeTwo>(TypeOne pageOne, TypeTwo pageTwo)
        {
            useIntervalPageOne = false;
            useIntervalPageTwo = false;

            if (pageOne != null)
            {
                intervalPageOne = pageOne;
                useIntervalPageOne = true;
            }

            if (pageTwo != null)
            {
                intervalPageTwo = pageTwo;
                useIntervalPageTwo = true;
            }

            randomInterval = (float)PartyOptions.activePartyOption.randomInterval;
            UpdateRandomIntervalTextBoxes();
        }

        /// <summary>
        /// Is called when a new random update interval was entered.
        /// </summary>
        /// <param name="newIntervall">The random update interval</param>
        public static void NewRandomInterval(float newIntervall)
        {
            randomInterval = newIntervall;
            UpdateRandomIntervalTextBoxes();
        }

        /// <summary>
        /// Updates the random update interval text boxes to value of randomInterval.
        /// </summary>
        public static void UpdateRandomIntervalTextBoxes()
        {
            if (useIntervalPageOne)
                intervalPageOne.SetRandomUpdateIntervalTextBox(randomInterval);

            if (useIntervalPageTwo)
                intervalPageTwo.SetRandomUpdateIntervalTextBox(randomInterval);
        }

        #endregion
        #region Output display

        /// <summary>
        /// Sets
        /// </summary>
        /// <param name="newPartyControl"></param>
        public static void GiveVariablesOutput(PartyControlAdvanced newPartyControlAdvanced)
        {
            partyControlAdvanced = newPartyControlAdvanced;
        }

        /// <summary>
        /// Sets the output display to the given values.
        /// </summary>
        /// <param name="brightness">The new brightness, can also be null if it shouldn't change</param>
        /// <param name="rgbColor">The new color, can also be null if it shouldn't change</param>
        /// <param name="rgbColor">The new colorTemperature, can also be null if it shouldn't change</param>
        public static async void UpdateOutputDisplay(int? brightness, RGBColor? rgbColor, int? colorTemperature)
        {
            //Runs the code in the UI thread.
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                if (partyControlAdvanced != null)
                {
                    if (rgbColor == null && colorTemperature == null && colorTemperature == null)
                        partyControlAdvanced.UpdateOutputDisplay(brightness, null, null);

                    if (rgbColor != null && colorTemperature == null)
                        partyControlAdvanced.UpdateOutputDisplay(brightness, ColorAssistant.ConvertRGBColorToColor((RGBColor)rgbColor), ColorAssistant.ConvertRGBColorToColor((RGBColor)rgbColor));

                    if (rgbColor == null && colorTemperature != null)
                        partyControlAdvanced.UpdateOutputDisplay(brightness, ColorAssistant.ConvertColorTemperatureToColor((int)colorTemperature), ColorAssistant.ConvertColorTemperatureToColor((int)colorTemperature));
                    
                    if (rgbColor != null && colorTemperature != null)
                        partyControlAdvanced.UpdateOutputDisplay(brightness, ColorAssistant.ConvertRGBColorToColor((RGBColor)rgbColor), ColorAssistant.ConvertColorTemperatureToColor((int)colorTemperature));
                }
            });
        }

        #endregion
    }
}
