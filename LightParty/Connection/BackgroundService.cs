using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using LightParty.Party;

namespace LightParty.Connection
{
    /// <summary>
    /// Starts, stops and handels the connection to the desktop extension (LightPartyBackgroundProcess) as a fullTrustProcess.
    /// </summary>
    class BackgroundService
    {
        private static bool shouldRun = false; //Whether or not the background process should run.
        private static bool runs = false; //Whether or not the background process actually runs and is connected. Will always be delayed in comparison with shouldRun.
        private static bool isCreating = false; //Whether or not the background process is currently being created.
        private static bool stopOnCreation = false;  //Whether or not the background graph should be disposed when the creation is done. Used to prevent errors.
        public static AppServiceConnection connection; //The AppServiceConnection that handels the connection between Light Party and the background process.
        public static BackgroundTaskDeferral appServiceDeferral = null; //Prevents the applications from being suspended.

        private static string backgroundServiceTask = ""; //Task that the background process should perform once it has started.

        /// <summary>
        /// Initializes the background task, if the required permission are present.
        /// </summary>
        /// <param name="task">Task that the background process should perform. Available: loopbackAudio</param>
        public static async void InitializeBackgroundService(string task)
        {
            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0) && !runs && !shouldRun)
            {
                isCreating = true;
                shouldRun = true;
                backgroundServiceTask = task;

                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            }
        }

        /// <summary>
        /// Send a stop request to the background process.
        /// </summary>
        public static async Task StopBackgroundService()
        {
            if (!runs || connection == null)
            {
                if (isCreating)
                    stopOnCreation = true;
                return;
            }

            shouldRun = false;

            ValueSet taskSet = new ValueSet();
            taskSet.Add("action", "stopTask");
            taskSet.Add("task", "process");

            AppServiceResponse response = await connection.SendMessageAsync(taskSet);

            if (response.Message != null)
            {
                if ((string)response.Message["check"] != "success")
                {
                    Debug.Write("Error while sending stop action to background task!");
                }
            }
        }

        /// <summary>
        /// Is called, when the app service connection is lost. If that happens unexpectedly, annother background service will be initialized.
        /// </summary>
        public static void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            runs = false;
            if (appServiceDeferral != null)
            {
                appServiceDeferral.Complete();
            }

            if (shouldRun)
            {
                InitializeBackgroundService(backgroundServiceTask);
            }
        }

        /// <summary>
        /// Is called, when an AppServiceTrigger calls the application. Transfers the task that should be performed by the background process.
        /// </summary>
        public static async void AppServiceTriggerEvent()
        {
            ValueSet taskSet = new ValueSet();
            taskSet.Add("action", "startTask");
            taskSet.Add("task", backgroundServiceTask);

            AppServiceResponse response = await connection.SendMessageAsync(taskSet);

            if ((string)response.Message["check"] != "success")
                Debug.Write("Error while sending action to background task!");

            connection.RequestReceived += RequestReceived;

            isCreating = false;
            runs = true;
            if (stopOnCreation)
            {
                await StopBackgroundService();
                stopOnCreation = false;
            }
        }

        /// <summary>
        /// Is callend, when the UWP application receives a request from the background process. Passes the data on and sends a validation response back.
        /// </summary>
        private static async void RequestReceived(AppServiceConnection connection, AppServiceRequestReceivedEventArgs args)
        {
            ValueSet checkSet = new ValueSet();
            string action = (string)args.Request.Message["action"];

            switch (action)
            {
                case "loopbackAudio":
                    double audioLevel = (double)args.Request.Message["audioLevel"];
                    AudioInput.NewRawSoundLevel(audioLevel);

                    checkSet.Add("check", "success");
                    break;
            }

            if (checkSet.Count != 0)
            {
                await args.Request.SendResponseAsync(checkSet);
            }
            else
            {
                checkSet.Add("check", "error");
            }
        }
    }

}
