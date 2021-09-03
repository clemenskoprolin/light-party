using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Foundation;

namespace LightParty.Services
{
    /// <summary>
    /// This calass handels Lifecyle events like resuming and suspending, as well as extended execution sessions which allow the application to keep running while it's minimized.
    /// </summary>
    class LifecyleAssistant
    {
        private static bool startMicrophoneInputOnResume = false; //Whether or not the microphone should be started when the application is being resumed.
        private static EventHandler onResume;

        private static ExtendedExecutionSession session; //The extended execution session which allows the application to keep running while it's minimized.
        /// <summary>
        /// Returns true, if the extended execution session is running.
        /// </summary>
        public static bool IsExtendedExecutionSessionRunning
        {
            get { return session != null; }
        }

        /// <summary>
        /// Is called when application execution is being suspended. Stops the microphone input when it's used.
        /// </summary>
        public static void SuspendApp()
        {
            if (Party.SoundInput.isCreating || Party.SoundInput.isListening)
            {
                Party.SoundInput.StopMicrophoneInputSafely();
                startMicrophoneInputOnResume = true;
            }
        }

        /// <summary>
        /// Allows other classes to add an event handlers, which is called when the application is being resumed.
        /// </summary>
        /// <param name="newOnResume"></param>
        public static void CallOnResume(EventHandler newOnResume)
        {
            onResume = newOnResume;
        }

        /// <summary>
        /// Is called when application execution is being resumed. Resets and starts the microphone input if it was used.
        /// </summary>
        public static void ResumeApp()
        {
            if (Party.SoundInput.wasListening)
                Party.SoundInput.ResetMicrophoneInput();

            if (startMicrophoneInputOnResume)
            {
                _ = Party.SoundInput.StartMicrophoneInputSafely();
                startMicrophoneInputOnResume = false;
            }

            if (onResume != null)
                onResume(null, null);
        }

        /// <summary>
        /// Requests a new extended execution session. The request must contain a reason and a human-readable description.
        /// </summary>
        /// <param name="reason">Reason for the new extended execution session in the ExtendedExecutionReason type</param>
        /// <param name="description">Human-readable description for the new extended execution session as a string</param>
        /// <returns></returns>
        public static async Task<bool> RequestExtendedExecutionSession(ExtendedExecutionReason reason, String description)
        {
            ClearSession();

            ExtendedExecutionSession newSession = new ExtendedExecutionSession();
            newSession.Reason = reason;
            newSession.Description = description;
            newSession.Revoked += (x,y) => 
            {
                ClearSession();
            };

            ExtendedExecutionResult result = await newSession.RequestExtensionAsync();
            if (result == ExtendedExecutionResult.Allowed)
            {
                session = newSession;
                return true;
            }
            else
            {
                newSession.Dispose();
                return false;
            }
        }

        /// <summary>
        /// Disposes the extended execution session, if it is running.
        /// </summary>
        public static void ClearSession()
        {
            if (session != null)
            {
                session.Dispose();
                session = null;
            }
        }
    }
}
