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
    class BackgroundService
    {
        private static bool shouldRun = false; //Whether or not the background process should run.
        private static bool runs = false; //Whether or not the background process actually run. Will always be delayed in comparison with shouldRun.
        private static bool isLaunching = false; //True while a background process launches.
        public static AppServiceConnection connection; //The AppServiceConnection that handels the connection between Light Party and the background process.
        public static BackgroundTaskDeferral appServiceDeferral = null;

        private static string backgroundServiceTask = ""; //Task that the background process should perform once it has started.

        public static async void InitializeBackgroundService(string task)
        {
            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0) && !runs && !shouldRun)
            {
                shouldRun = true;
                isLaunching = true;
                backgroundServiceTask = task;

                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
                isLaunching = false;
                runs = true;
            }
        }

        public static async Task StopBackgroundService()
        {
            if (!runs || connection == null)
                return;

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

        public static async void AppServiceTriggerEvent()
        {
            ValueSet taskSet = new ValueSet();
            taskSet.Add("action", "startTask");
            taskSet.Add("task", backgroundServiceTask);

            AppServiceResponse response = await connection.SendMessageAsync(taskSet);

            if ((string)response.Message["check"] != "success")
                Debug.Write("Error while sending action to background task!");

            connection.RequestReceived += RequestReceived;
        }

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
