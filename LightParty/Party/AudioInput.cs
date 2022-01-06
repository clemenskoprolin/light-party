using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightParty.Party
{
    /// <summary>
    /// This class handles the audio input with microphone or loopback and calculates the current audio level.
    /// </summary>
    class AudioInput
    {
        public static bool isListening = false;
        private static List<double> lastSoundLevels = new List<double>(); //Contains the last sound levels and is used to calculate the average.

        /// <summary>
        /// Starts the microphone or loopback input and trys not to crash the application.
        /// </summary>
        public async static Task StartAudioInputSafely()
        {
            if (PartyOptions.activePartyOption.audioSource == 0)
            {
                await MicrophoneInput.StartMicrophoneInputSafely();
            }
            if (PartyOptions.activePartyOption.audioSource == 1)
            {
                Connection.BackgroundService.InitializeBackgroundService("loopbackAudio");
            }

            isListening = true;
        }

        /// <summary>
        /// Stops the microphone or loopback input and trys not to crash the application.
        /// </summary>
        public static async Task StopAudioInputSafely()
        {
            if (PartyOptions.activePartyOption.audioSource == 0)
            {
                MicrophoneInput.StopMicrophoneInputSafely();
            }
            if (PartyOptions.activePartyOption.audioSource == 1)
            {
                await Connection.BackgroundService.StopBackgroundService();
            }

            isListening = false;
        }

        /// <summary>
        /// Starts and stops the microphone input and background service as given perPartyOptions.activePartyOption.audioSource.
        /// </summary>
        public static async Task UpdateAudioInputMethod()
        {
            if (PartyOptions.activePartyOption.audioSource == 0)
            {
                await Connection.BackgroundService.StopBackgroundService();
                await Task.Delay(200);
                await MicrophoneInput.StartMicrophoneInputSafely();
            }
            if (PartyOptions.activePartyOption.audioSource == 1)
            {
                MicrophoneInput.StopMicrophoneInputSafely();
                await Task.Delay(200);
                Connection.BackgroundService.InitializeBackgroundService("loopbackAudio");
            }
        }

        /// <summary>
        /// Converts the raw audio level to a audio level from 0 to 100 and calls the necessary following methods. Is either called by an audio frame update or by BackgroundService.
        /// </summary>
        /// <param name="rawLevel"></param>
        public static void NewRawSoundLevel(double rawLevel)
        {
            rawLevel += 100;
            rawLevel = rawLevel < 0 ? 0 : rawLevel;
            rawLevel = rawLevel > 100 ? 100 : rawLevel;

            if (!Double.IsNaN(rawLevel))
            {
                double soundLevel = CalculateAverage(rawLevel);

                LightProcessingSoundInput.NewSoundLevel(soundLevel);
                PartyUIUpdater.NewSoundLevel(soundLevel);
            }
        }

        /// <summary>
        /// Calculates the average of the last 8 sound levels.
        /// </summary>
        /// <param name="newSoundLevel">The new sound level</param>
        /// <returns>The average of the last 8 sound levels</returns>
        private static double CalculateAverage(double newSoundLevel)
        {
            lastSoundLevels.Add(newSoundLevel);

            if (lastSoundLevels.Count < 8)
            {
                return newSoundLevel;
            }
            else
            {
                double average = lastSoundLevels.Average();

                lastSoundLevels.RemoveAt(0);
                return average;
            }
        }
    }
}
