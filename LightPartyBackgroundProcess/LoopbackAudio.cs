using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using NAudio.Wave;

namespace LightPartyBackgroundProcess
{
    /// <summary>
    /// This class captures and sends the raw audio level of the desktop with the help of WasapiLoopbackCapture by Naudio.
    /// </summary>
    class LoopbackAudio
    {
        static WasapiLoopbackCapture capture; //Contains the WasapiLoopbackCapture by Naudio.
        static Thread captureThread; //Dedicated thread for loopback capture to keep COM objects alive.

        /// <summary>
        /// Starts WasapiLoopbackCapture via Naudio on a dedicated STA thread.
        /// </summary>
        public static void StartLoopbackCapture()
        {
            captureThread = new Thread(() =>
            {
                try
                {
                    capture = new WasapiLoopbackCapture();
                    capture.DataAvailable += DataAvailable;

                    capture.RecordingStopped += (sender, eventArgs) =>
                    {
                        capture.Dispose();
                    };

                    capture.StartRecording();

                    // Keep the thread alive so COM objects remain valid.
                    while (capture != null)
                    {
                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error starting loopback capture: " + ex.Message);
                }
            });
            captureThread.SetApartmentState(ApartmentState.STA);
            captureThread.IsBackground = true;
            captureThread.Start();
        }

        /// <summary>
        /// Stops the loopback capture if it has been initialized.
        /// </summary>
        public static void StopLoopbackCapture()
        {
            if (capture != null)
            {
                capture.StopRecording();
                capture = null; // Signals the captureThread to exit.
            }
        }

        /// <summary>
        /// Is called by WasapiLoopbackCapture periodically when data is available. Calculates the raw audio level from it and passes it to SendLoopbackAudioLevel on.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        static void DataAvailable(object sender, WaveInEventArgs eventArgs)
        {
            WaveBuffer buffer = new WaveBuffer(eventArgs.Buffer);

            double audioLevel = 0f;
            for (int i = 0; i < eventArgs.BytesRecorded / 4; i++)
            {
                float currentSample = Math.Abs(buffer.FloatBuffer[i]);
                audioLevel += Math.Abs(currentSample);
            }
            audioLevel = Math.Log10(audioLevel / eventArgs.BytesRecorded) * 20;

            if (!Double.IsNaN(audioLevel))
            {
                SendLoopbackAudioLevel(audioLevel);
            }
        }

        /// <summary>
        /// Send the raw audio via the app service to the UWP application (--> Connection.BackgroundService.RequestReceived)
        /// </summary>
        /// <param name="audioLevel">The raw audio</param>
        static async void SendLoopbackAudioLevel(double audioLevel)
        {
            ValueSet taskSet = new ValueSet();
            taskSet.Add("action", "loopbackAudio");
            taskSet.Add("audioLevel", audioLevel);

            AppServiceResponse response = await Program.connection.SendMessageAsync(taskSet);

            if ((string)response.Message["check"] != "success")
                Console.Write("Error while sending action to Light Party!");
        }
    }
}
