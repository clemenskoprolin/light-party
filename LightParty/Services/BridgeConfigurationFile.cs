using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Data.Json;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
using LightParty.Connection;

namespace LightParty.Services
{
    /// <summary>
    /// This class contains all methods to save the data of the brige to a file and read from it.
    /// </summary>
    class BridgeConfigurationFile
    {
        private static StorageFolder storageFolder = ApplicationData.Current.LocalCacheFolder; //Folder, in which the file is saved.

        public static BridgeConfigurationData bridgeConfigurationData = new BridgeConfigurationData(); //Data that is stored in the file.
        private static StorageFile brigeConfigurationFile; //The file which contains the data.
        private static string bridgeConfigurationFileName = "bridgeConfiguration"; //Name of the file.

        /// <summary>
        /// Checks if the bridge configuration file already exists.
        /// </summary>
        /// <returns>Whether or not the file exists</returns>
        public static async Task<bool> CheckForBridgeConfiguration()
        {
            var item = await storageFolder.TryGetItemAsync(bridgeConfigurationFileName);

            bool result = item != null;
            return result;
        }

        /// <summary>
        /// Creates the bridge configuration file in the storage folder.
        /// </summary>
        public static async Task CreateBridgeConfiguration()
        {
            if (BridgeInformation.demoMode)
                return;

            brigeConfigurationFile = await storageFolder.CreateFileAsync(bridgeConfigurationFileName, CreationCollisionOption.ReplaceExisting);
        }

        /// <summary>
        /// Updates the bridge configuration file with data from bridgeConfigurationData.
        /// </summary>
        public static async Task UpdateBridgeConfigurationFile()
        {
            if (BridgeInformation.demoMode)
                return;

            //Creates a json string from the data.
            JsonObject jsonObject = new JsonObject();

            jsonObject["bridgeIPAddress"] = JsonValue.CreateStringValue(bridgeConfigurationData.bridgeIPAddress);
            jsonObject["bridgeAppKey"] = JsonValue.CreateStringValue(bridgeConfigurationData.bridgeAppKey);

            string jsonString = jsonObject.Stringify();

            //Encryptes the json string.
            byte[] encryptedBytes = CryptographyAssistant.EncryptStringAes(jsonString);

            //Writes the encrypted bytes to the brige configuration file.
            brigeConfigurationFile = await storageFolder.GetFileAsync(bridgeConfigurationFileName);

            using (var stream = await brigeConfigurationFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                stream.Size = 0;
                using (var outputStream = stream.GetOutputStreamAt(0))
                {
                    using (var dataWriter = new DataWriter(outputStream))
                    {
                        dataWriter.WriteBytes(encryptedBytes);
                        await dataWriter.StoreAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Reads the bytes from the brige configuration file, decryptes them and trys to save the result in bridgeConfigurationData.
        /// </summary>
        /// <returns>Whether or not all expected data of the file could be read</returns>
        public static async Task<bool> ReadBridgeConfigurationFile()
        {
            if (BridgeInformation.demoMode)
                return true;

            bool completelySuccessful = true; //Is set to false, when when one or all variable cannot be readed.
            brigeConfigurationFile = await storageFolder.GetFileAsync(bridgeConfigurationFileName);

            byte[] fileInput;
            using (Stream stream = await brigeConfigurationFile.OpenStreamForReadAsync())
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    fileInput = memoryStream.ToArray();
                }
                stream.Dispose();
            }

            try
            {
                string jsonInput = CryptographyAssistant.DecryptStringAes(fileInput);
                JsonObject jsonObject = JsonObject.Parse(jsonInput);
                try
                {
                    bridgeConfigurationData.bridgeIPAddress = jsonObject.GetNamedString("bridgeIPAddress");
                }
                catch
                {
                    bridgeConfigurationData.bridgeIPAddress = "";
                    completelySuccessful = false;
                }

                try
                {
                    bridgeConfigurationData.bridgeAppKey = jsonObject.GetNamedString("bridgeAppKey");
                }
                catch
                {
                    bridgeConfigurationData.bridgeAppKey = "";
                    completelySuccessful = false;
                }
            }
            catch 
            {
                completelySuccessful = false;
            };

            return completelySuccessful;
        }

        /// <summary>
        /// Resets the bridge configuration temporarily. This does not delete or overwrite the bridge configuration file.
        /// </summary>
        public static void ResetBridgeConfigurationTemporarily()
        {
            bridgeConfigurationData.bridgeIPAddress = "";
            bridgeConfigurationData.bridgeAppKey = "";
        }
    }

    public class BridgeConfigurationData
    {
        public string bridgeIPAddress = "";
        public string bridgeAppKey = "";
    }
}
