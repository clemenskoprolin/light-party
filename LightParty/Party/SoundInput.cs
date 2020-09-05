using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Windows.Media.Audio;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
using Windows.Media.Transcoding;
using Windows.Media;
using Windows.Foundation;
using Windows.UI.Core;

namespace LightParty.Party
{
    /// <summary>
    /// This class handles the audio input and calculates the current audio level.
    /// </summary>
    class SoundInput
    {
        public static bool isCreating = false; //Whether or not the audio graph is currently being created.
        public static bool isListing = false; //Whether or not the audio graph is started.
        public static bool stopOnCreation = false; //Whether or not the audio graph should be deleted when the creation is done. Used to prevent errors.
        private static List<double> lastSoundLevels = new List<double>(); //Contains the last sound levels and is used to calculate the average.

        private static AudioGraph graph; //The audio graph for the operations.
        private static AudioDeviceInputNode deviceInputNode; //The input node for the default microphone.
        private static AudioFrameOutputNode frameOutputNode; //The frame output node for the calculation of the sound level.

        //Because every computer has a different number of samples per quantum, this class does not process every single frame to a sound level.
        //Instead, it only processes as much frames as it's needed, to reach samplesPerQuantumLimit.
        private static int samplesPerQuantumLimit = 480; //Limit of the samples per quantum. audioFrameUpdateMinimum is calculated with this number.
        private static int audioFrameUpdateMinimum; //When this number is reached by audioFrameUpdateCount, the class processes the sound level.
        private static int audioFrameUpdateCount = 0; //Is increased every time, a new audio frame is recognized.

        /// <summary>
        /// Starts the microphone input.
        /// </summary>
        /// <returns>Whether or not the start was successful</returns>
        public static async Task<bool> StartInput()
        {
            isCreating = true;

            bool successAudioGraph = true;
            bool successOutgoingConnection = true;

            if (graph == null)
                successAudioGraph = await CreateAudioGraph();

            if (deviceInputNode == null && frameOutputNode == null)
            {
                successOutgoingConnection = await CreateNodes();
            }
            else
            {
                deviceInputNode.Start();
                frameOutputNode.Start();
            }                          

            if (successAudioGraph && successOutgoingConnection)
            {
                graph.Start();

                isListing = true;
                isCreating = false;

                if (stopOnCreation)
                {
                    _ = StopInput();
                    stopOnCreation = false;
                }
                return true;
            }
            else
            {
                isCreating = false;
                isListing = false;
                return false;
            }
        }

        /// <summary>
        /// Stops the mircrophone input by stopping the audio graph and resetting the nodes.
        /// </summary>
        public static async Task StopInput()
        {
            //Is used to prevent errors.
            if (graph.CompletedQuantumCount < 500)
                await Task.Delay(100);

            //Runs the code in the UI thread. This is used to prevent error, too.
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                deviceInputNode.Stop();
                frameOutputNode.Stop();
                graph.Stop();

                isListing = false;
            });
        }

        /// <summary>
        /// Trys to create the audio graph.
        /// </summary>
        /// <returns>Whether or not the creation was successful</returns>
        private static async Task<bool> CreateAudioGraph()
        {
            try
            {
                AudioGraphSettings settings = new AudioGraphSettings(AudioRenderCategory.Media);
                settings.QuantumSizeSelectionMode = QuantumSizeSelectionMode.LowestLatency;

                CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);
                graph = result.Graph;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Tyrs to creates the frame output node and trys to set the outgoing connection to it. Also calculates audioFrameUpdateMinimum.
        /// </summary>
        /// <returns>Whether or not the attempt was successful</returns>
        private static async Task<bool> CreateNodes()
        {
            try
            {
                CreateAudioDeviceInputNodeResult deviceInputNodeResult = await graph.CreateDeviceInputNodeAsync(MediaCategory.Other);
                deviceInputNode = deviceInputNodeResult.DeviceInputNode;

                frameOutputNode = graph.CreateFrameOutputNode(graph.EncodingProperties);
                graph.QuantumStarted += Graph_QuantumStarted;

                audioFrameUpdateMinimum = Convert.ToInt32(samplesPerQuantumLimit / graph.SamplesPerQuantum);
                deviceInputNode.AddOutgoingConnection(frameOutputNode);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Used for the calculation of the raw audio level.
        /// </summary>
        [ComImport]
        [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        unsafe interface IMemoryBufferByteAccess
        {
            void GetBuffer(out byte* buffer, out uint capacity);
        }

        /// <summary>
        /// When audioFrameUpdateMinimum is reached by audioFrameUpdateCount, this method gets the current audio frame, obtains the data from it
        /// and calculates the raw audio level from -100 to 0.
        /// </summary>
        private static unsafe void Graph_QuantumStarted(AudioGraph sender, object args)
        {
            audioFrameUpdateCount++;
            if (audioFrameUpdateCount >= audioFrameUpdateMinimum)
            {
                AudioFrame audioFrame = frameOutputNode.GetFrame();
                float[] floatData;
                using (AudioBuffer audioBuffer = audioFrame.LockBuffer(AudioBufferAccessMode.Write))
                using (IMemoryBufferReference reference = audioBuffer.CreateReference())
                {
                    ((IMemoryBufferByteAccess)reference).GetBuffer(out byte* dataInBytes, out uint capacity);

                    float* unsafeFloatData = (float*)dataInBytes;
                    floatData = new float[capacity / sizeof(float)];

                    for (int i = 0; i < capacity / sizeof(float); i++)
                    {
                        floatData[i] = unsafeFloatData[i];
                    }
                }

                double soundLevel = 0f;
                foreach (float sample in floatData)
                {
                    soundLevel += Math.Abs(sample);
                }
                soundLevel = Math.Log10(soundLevel / floatData.Length) * 20;

                NewRawSoundLevel(soundLevel);

                audioFrameUpdateCount = 0;
            }
        }

        /// <summary>
        /// Converts the raw audio level to a audio level from 0 to 100 and calls the necessary following methods.
        /// </summary>
        /// <param name="rawLevel"></param>
        private static void NewRawSoundLevel(double rawLevel)
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