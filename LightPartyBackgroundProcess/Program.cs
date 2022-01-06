using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace LightPartyBackgroundProcess
{
    /// <summary>
    /// This class handels the conncetion to the UWP application.
    /// </summary>
    class Program
    {
        public static AppServiceConnection connection = null;

        static void Main(string[] args)
        {
            StartAppServiceConnection();
            Process.GetCurrentProcess().WaitForExit();
        }

        /// <summary>
        /// Starts the app service connection.
        /// </summary>
        static async void StartAppServiceConnection()
        {
            connection = new AppServiceConnection();
            connection.AppServiceName = "LightPartyBackgroundAppService";
            connection.PackageFamilyName = Package.Current.Id.FamilyName;
            connection.RequestReceived += RequestReceived;
            connection.ServiceClosed += ServiceClosed;

            AppServiceConnectionStatus status = await connection.OpenAsync();
            if (status != AppServiceConnectionStatus.Success)
            {
                Console.WriteLine("An error occurred while trying to establish the app service connection: " + status.ToString());
            }

        }

        /// <summary>
        /// Is callend, when this process receives a request from the UWP application. Passes the data on and sends a validation response back.
        /// </summary>
        static async void RequestReceived(AppServiceConnection connection, AppServiceRequestReceivedEventArgs args)
        {
            ValueSet checkSet = new ValueSet();

            string action = (string)args.Request.Message["action"];
            string startTask = "";
            string stopTask = "";

            switch (action)
            {
                case "startTask":
                    startTask = (string)args.Request.Message["task"];
                    break;
                
                case "stopTask":
                    stopTask = (string)args.Request.Message["task"];
                    break;
            }

            switch (startTask)
            {
                case "loopbackAudio":
                    checkSet.Add("check", "success");
                    LoopbackAudio.StartLoopbackCapture();
                    break;
            }
            
            switch (stopTask)
            {
                case "loopbackAudio":
                    checkSet.Add("check", "success");
                    LoopbackAudio.StopLoopbackCapture();
                    break;

                case "process":
                    LoopbackAudio.StopLoopbackCapture();
                    checkSet.Add("check", "success");
                    await args.Request.SendResponseAsync(checkSet);

                    Environment.Exit(0);
                    break;
            }

            if (checkSet.Count != 0)
            {
                await args.Request.SendResponseAsync(checkSet);
            } else
            {
                checkSet.Add("check", "error");
            }
        }

        /// <summary>
        /// Is called when the connection to the UWP application is lost. Stops this process
        /// </summary>
        static void ServiceClosed(AppServiceConnection connection, AppServiceClosedEventArgs args)
        {
            Environment.Exit(0);
        }
    }
}
