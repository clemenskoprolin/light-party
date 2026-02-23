using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Data.Json;
using Windows.Web.Http;
using LightParty.Connection;
using LightParty.Services;

namespace LightParty.Services
{
    /// <summary>
    /// DEPRECATED and now longer used as of version 2.2 except SetUseTelemetry and HasTelemetryConfig for backwards compatibility.
    /// This class contains the methods that communicate with the telemetery servers of this application.
    /// </summary>
    class TelemetryService
    {
        public static bool? useTelemetry = null; //Whether or not regular telemetry reports should be send.
        private static bool sentFirstStartTelemetry = false; //Whether or not the telemetry report on the first application start was sent.
        public static bool sentTelemetryReport = false; //Whether or not the telemetry report on the application start was sent.

        private static string firstTimeTelemetryURL = "https://koprolin.com/lightparty/php/telemetry/firststart.php"; //URL to which a request is sent after the user agreed to the Privacy Policy.
        private static string telemetryReportURL = "https://koprolin.com/lightparty/php/telemetry/startreport.php"; //URL to which a request is sent when telemetry is enabled and the application has started.
        private static string locationServiceURL = "https://www.cloudflare.com/cdn-cgi/trace"; //URL to service which is used to obtain the user's country and state. It's only used when telemetry is enabled.

        /// <summary>
        /// Sets useTelemetry and saves the option in configuration file, too.
        /// </summary>
        /// <param name="value">Whether useTelemetry should be enabled or disabled.</param>
        public static async void SetUseTelemetry(bool value)
        {
            useTelemetry = value;

            ConfigurationFile.configurationData.useTelemetry = useTelemetry;
            await ConfigurationFile.UpdateConfigurationFile();
        }

        /// <summary>
        /// Trys to load the telemetry config.
        /// </summary>
        /// <returns>Whether or not the config could be found</returns>
        public static bool HasTelemetryConfig()
        {
            useTelemetry = ConfigurationFile.configurationData.useTelemetry;
            return useTelemetry != null;
        }

        /// <summary>
        /// DEPRECATED as of version 2.2.
        /// Trys to send a telemetry report which does not contain any information of the user.
        /// This method is called after the user agreed to the Privacy Policy. This process cannot be deactivated.
        /// </summary>
        /// <returns>Whether or not the request was successfully send</returns>
        public static async Task<bool> SendFirstStartTelemetry()
        {
            if (BridgeInformation.demoMode || sentFirstStartTelemetry)
                return false;

            //It is probably not the best idea to use 'verifed: true' in the content of a request as verification.
            //However, it's just a very basic protection so that not everyone can just open the URL in the Browser and maniplate the collected statistics.
            string response = await InternetRequests.SendJsonPostRequest(firstTimeTelemetryURL, "{ \"verifed\": true }");

            sentFirstStartTelemetry = response != "null";
            return sentFirstStartTelemetry;
        }

        /// <summary>
        /// DEPRECATED as of version 2.2.
        /// Trys to send a telemetry report which contains the user's country and state. In order to obtain this information, this application uses a service from Cloudflare.
        /// This method is called after the user started the application and when telemetry is activated.
        /// For more information you can read the summary of the Privacy Policy: https://koprolin.com/lightparty/legal/
        /// </summary>
        /// <returns>Whether or not the report was successfully send</returns>
        public static async Task<bool> SendTelemetryReport()
        {
            if (BridgeInformation.demoMode || !(bool)useTelemetry || sentTelemetryReport)
                return false;

            string[] userLocation = await GetUserLocation();

            //Creates a json string from the location data.
            JsonObject jsonObject = new JsonObject();

            jsonObject["state"] = JsonValue.CreateStringValue(userLocation[0]);
            jsonObject["country"] = JsonValue.CreateStringValue(userLocation[1]);

            string jsonString = jsonObject.Stringify();
            string response = await InternetRequests.SendJsonPostRequest(telemetryReportURL, jsonString);

            sentTelemetryReport = response != "null";
            return sentTelemetryReport;
        }

        /// <summary>
        /// DEPRECATED as of version 2.2.
        /// This method uses a service from Cloudflare to obtain user's country and state. 
        /// For more information you can read the summary of the Privacy Policy of this application: https://koprolin.com/lightparty/legal/
        /// And the Privacy Policy of Cloudflare: https://www.cloudflare.com/privacypolicy/
        /// </summary>
        /// <returns>A array of strings. The fist value is the user's state, the second is the user's country.</returns>
        public static async Task<string[]> GetUserLocation()
        {
            string state = "";
            string country = "";

            string locationServiceData = await InternetRequests.SendGetRequest(locationServiceURL);

            if (locationServiceData != "null")
            {
                using (System.IO.StringReader reader = new System.IO.StringReader(locationServiceData))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Contains("colo"))
                        {
                            state = line.Split("=")[1];
                        }

                        if (line.Contains("loc"))
                        {
                            country = line.Split("=")[1];
                        }
                    }
                }
            }

            string[] location = { state, country };
            return location;
        }
    }
}
