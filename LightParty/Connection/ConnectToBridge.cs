using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightParty.Services;
using Q42.HueApi;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models.Bridge;

namespace LightParty.Connection
{
    /// <summary>
    /// This class contains the methods that locate the bridge and establish a connection with it.
    /// </summary>
    class ConnectToBridge
    {
        static private string ipAdress = ""; //Contains the temporary IP address of the connected bridge
        static private string appKey = ""; //Contains the temporary app key of the current connection

        #region Location

        /// <summary>
        /// Trys to load the IP address of the last connected bridge and stores it in ipAdress.
        /// </summary>
        /// <returns>Whether or not a IP Address could be found</returns>
        public static bool HasIPAddress()
        {
            ipAdress = BridgeConfigurationFile.bridgeConfigurationData.bridgeIPAddress;
            return ipAdress != "";
        }

        /// <summary>
        /// Trys to locates all bridges in the network.
        /// </summary>
        /// <returns>The list of the found briges</returns>
        public static async Task<LocatedBridge[]> LocateAllBridges()
        {
            if (BridgeInformation.demoMode)
                return AddTestingBridges(new LocatedBridge[] { }, 50);

            try
            {
                IBridgeLocator bridgeLocator = new HttpBridgeLocator();

                var bridges = await bridgeLocator.LocateBridgesAsync(TimeSpan.FromSeconds(5));
                LocatedBridge[] bridgeList = bridges.Cast<LocatedBridge>().ToArray();

                if (BridgeInformation.demoMode)
                    bridgeList = AddTestingBridges(bridgeList, 50);

                return bridgeList;
            }
            catch
            {
                return new LocatedBridge[] { };
            }
        }

        /// <summary>
        /// Only for test purposes! Adds randomly generated new bridges to bridgeArray
        /// </summary>
        /// <param name="bridgeArray">Array of already found bridges</param>
        /// <param name="number">How many randomly generated bridges should be added</param>
        /// <returns>The array of bridges with new randomly generated new bridges</returns>
        static LocatedBridge[] AddTestingBridges(LocatedBridge[] bridgeArray, int number)
        {
            List<LocatedBridge> bridgeList = bridgeArray.ToList<Q42.HueApi.Models.Bridge.LocatedBridge>();

            Random random = new Random();
            for (int i = 0; i < number; i++)
            {
                string newIPAdress = random.Next(0, 999) + "." + random.Next(0, 999) + "." + random.Next(0, 999) + "." + random.Next(0, 999);
                string newBridgeId = "DEMO_MODE";
                bridgeList.Add(new LocatedBridge { IpAddress = newIPAdress, BridgeId = newBridgeId });
            }
            return bridgeList.ToArray();
        }

        /// <summary>
        /// Trys to restablish the connection with the last connected bridge
        /// </summary>
        /// <returns>Whether or not the attempt was successful</returns>
        public static bool SelectBridgeWithSavedIPAddress()
        {
            return SelectBridge(ipAdress);
        }

        /// <summary>
        /// Trys to establish a connection with a bridge and saves the client in BridgeInformation.client.
        /// </summary>
        /// <param name="ipAddress">The IP Address of the bridge</param>
        /// <returns>Whether or not the attempt was successful</returns>
        public static bool SelectBridge(string ipAddress)
        {
            try
            {
                BridgeInformation.client = new LocalHueClient(ipAddress);
                SaveIPAddress(ipAddress);

                return true;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// Saves a new IP Address of a bridge in the brige configuration file.
        /// </summary>
        /// <param name="ipAddress">A new IP Address of a bridge</param>
        private static void SaveIPAddress(string ipAddress)
        {
            if (ipAdress != ipAddress)
            {
                BridgeConfigurationFile.bridgeConfigurationData.bridgeIPAddress = ipAddress;
                _ = BridgeConfigurationFile.UpdateBridgeConfigurationFile();

                ipAdress = ipAddress;
            }
        }

        /// <summary>
        /// Returns the IP adress of the last successfully connected bridge.
        /// </summary>
        /// <returns>The IP adress of the last successfully connected bridge</returns>
        private static string GetIPAddress()
        {
            return ipAdress;
        }

        #endregion
        #region Registration

        /// <summary>
        /// Trys to register the application 'LightParty' with the name of the computer and stores the app key in appKey.
        /// </summary>
        /// <returns>Whether or not the application could be registered</returns>
        public static async Task<bool> RegisterApplication()
        {
            if (BridgeInformation.demoMode)
            {
                appKey = "demo-mode";
                SaveNewAppKey(appKey);

                BridgeInformation.isConnected = true;
                return true;
            }

            try
            {
                appKey = await BridgeInformation.client.RegisterAsync("LightParty", GetDeviceName());
                SaveNewAppKey(appKey);

                BridgeInformation.isConnected = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the device name which is used to register the device at the Bridge.
        /// </summary>
        /// <returns>The device name which is used to register the device at the Bridge</returns>
        private static string GetDeviceName()
        {
            string deviceName = Environment.MachineName.Trim();
            deviceName = deviceName.Replace(" ", "");

            if (deviceName.Length > 15)
            {
                deviceName = deviceName.Substring(0, 15);
            }

            if (deviceName.Length > 0)
            {
                return deviceName;
            }
            else
            {
                return "Windows10Device";
            }
        }

        #endregion
        #region Initialization

        /// <summary>
        /// Trys to load the app key of the last connection and stores it in appKey.
        /// </summary>
        /// <returns>Whether or not a app key could be found</returns>
        public static bool HasAppKey()
        {
            appKey = BridgeConfigurationFile.bridgeConfigurationData.bridgeAppKey;

            return appKey != "";
        }

        /// <summary>
        /// Trys to initialize the app key.
        /// </summary>
        /// <returns>Whether or not the initialization was successful</returns>
        public static bool InitializeKey()
        {
            if (BridgeInformation.demoMode)
            {
                BridgeInformation.isConnected = true;
                SaveNewAppKey(appKey);

                return true;
            }

            if (appKey == "")
                return false;

            try
            {
                BridgeInformation.client.Initialize(appKey);
                SaveNewAppKey(appKey);

                BridgeInformation.isConnected = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Saves a new app key of a connection in the brige configuration file.
        /// </summary>
        /// <param name="newAppKey">A new app key</param>
        private static void SaveNewAppKey(string newAppKey)
        {
            BridgeConfigurationFile.bridgeConfigurationData.bridgeAppKey = newAppKey;
            _ = BridgeConfigurationFile.UpdateBridgeConfigurationFile();
        }

        /// <summary>
        /// Returns the app key of the current connection.
        /// </summary>
        /// <returns>The app key of the current connection</returns>
        public static string GetAppKey()
        {
            return appKey;
        }

        #endregion
    }
}
