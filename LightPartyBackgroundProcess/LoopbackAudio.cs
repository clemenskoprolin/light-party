using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using NAudio.Wave;

namespace LightPartyBackgroundProcess
{
    class LoopbackAudio
    {
        static WasapiLoopbackCapture capture;

        public static void StartLoopbackCapture()
        {
            capture = new WasapiLoopbackCapture();
            capture.DataAvailable += DataAvailable;

            capture.RecordingStopped += (sender, eventArgs) =>
            {
                capture.Dispose();
                Console.WriteLine("Recording stopped");
            };

            capture.StartRecording();
        }

        public static void StopLoopbackCapture()
        {
            if (capture != null)
                capture.StopRecording();
        }

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
