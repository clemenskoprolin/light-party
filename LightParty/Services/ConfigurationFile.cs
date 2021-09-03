using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Data.Json;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
using LightParty.Connection;
using System;
using System.Diagnostics;

namespace LightParty.Services
{
    class ConfigurationFile
    {
        private static StorageFolder storageFolder = ApplicationData.Current.LocalFolder; //Folder, in which the file is saved.

        public static ConfigurationData configurationData = new ConfigurationData(); //Data that is stored in the file.
        private static StorageFile configurationFile; //The file which contains the data.
        private static string configurationFileName = "configuration"; //Name of the file.

        /// <summary>
        /// Checks if the configuration file already exists.
        /// </summary>
        /// <returns>Whether or not the file exists</returns>
        public static async Task<bool> CheckForConfiguration()
        {
            try
            {
                var item = await storageFolder.TryGetItemAsync(configurationFileName);

                bool result = item != null;
                return result;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates the configuration file in the storage folder.
        /// </summary>
        public static async Task CreateConfiguration()
        {
            if (BridgeInformation.demoMode)
                return;

            configurationFile = await storageFolder.CreateFileAsync(configurationFileName, CreationCollisionOption.ReplaceExisting);
        }

        /// <summary>
        /// Updates the configuration file with data from configurationData.
        /// </summary>
        public static async Task UpdateConfigurationFile()
        {
            if (BridgeInformation.demoMode)
                return;

            //Creates a json string from the data.
            JsonObject jsonObject = new JsonObject();

            if (configurationData.useTelemetry != null)
                jsonObject["useTelemetry"] = JsonValue.CreateBooleanValue((bool)configurationData.useTelemetry);
            else
                jsonObject["useTelemetry"] = JsonValue.CreateNullValue();

            if (configurationData.popupClosedVersion != "")
                jsonObject["popupClosedVersion"] = JsonValue.CreateStringValue(configurationData.popupClosedVersion);
            else
                jsonObject["popupClosedVersion"] = JsonValue.CreateNullValue();

            string jsonString = jsonObject.Stringify();
            Debug.WriteLine(jsonString);

            //Encodes the string to UTF8.
            System.Text.UnicodeEncoding encoding = new System.Text.UnicodeEncoding();
            byte[] encodedString = encoding.GetBytes(jsonString);

            //Writes the decoded bytes to the configuration file.
            configurationFile = await storageFolder.GetFileAsync(configurationFileName);

            using (var stream = await configurationFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                stream.Size = 0;
                using (var outputStream = stream.GetOutputStreamAt(0))
                {
                    using (var dataWriter = new DataWriter(outputStream))
                    {
                        dataWriter.WriteBytes(encodedString);
                        await dataWriter.StoreAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Reads the bytes from the configuration file, decodes them and trys to save the result in configurationData.
        /// </summary>
        /// <returns>Whether or not all expected data of the file could be read</returns>
        public static async Task<bool> ReadConfigurationFile()
        {
            if (BridgeInformation.demoMode)
                return true;

            bool completelySuccessful = true; //Is set to false, when when one or all variable cannot be readed.
            configurationFile = await storageFolder.GetFileAsync(configurationFileName);

            string jsonInput;
            using (Stream stream = await configurationFile.OpenStreamForReadAsync())
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.Unicode))
                {
                    jsonInput = reader.ReadToEnd();
                }
            }

            try
            {
                JsonObject jsonObject = JsonObject.Parse(jsonInput);

                try
                {
                    configurationData.useTelemetry = jsonObject.GetNamedBoolean("useTelemetry");
                    configurationData.popupClosedVersion = jsonObject.GetNamedString("popupClosedVersion");
                }
                catch
                {
                    completelySuccessful = false;
                }
            }
            catch
            {
                completelySuccessful = false;
            }

            return completelySuccessful;
        }

        /// <summary>
        /// Resets the configuration temporarily. This does not delete or overwrite the bridge configuration file.
        /// </summary>
        public static void ResetConfigurationTemporarily()
        {
            configurationData.useTelemetry = null;
        }
    }
}

public class ConfigurationData
{
    public bool? useTelemetry = null;
    public string popupClosedVersion = "";
}