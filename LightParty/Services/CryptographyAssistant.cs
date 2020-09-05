using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LightParty.Services
{
    /// <summary>
    /// This class contain all methods of this application that encrypt or decrypt data.
    /// </summary>
    class CryptographyAssistant
    {
        /// <summary>
        /// Encryptes a string with the Advanced Encryption Standard and returns the result as an array of bytes.
        /// </summary>
        /// <param name="input">The string which will be encrypted</param>
        /// <returns>The encrypted string as an array of bytes</returns>
        public static byte[] EncryptStringAes(string input)
        {
            byte[] encryptedBytes;

            using (Aes aesAlgorithm = Aes.Create())
            {
                aesAlgorithm.Key = GetKey();
                aesAlgorithm.IV = GetInitVector();

                ICryptoTransform encryptor = aesAlgorithm.CreateEncryptor(aesAlgorithm.Key, aesAlgorithm.IV);

                using (MemoryStream memoryStreamEncrypt = new MemoryStream())
                {
                    using (CryptoStream cryptoStreanEncrypt = new CryptoStream(memoryStreamEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriterEncrypt = new StreamWriter(cryptoStreanEncrypt))
                        {
                            streamWriterEncrypt.Write(input);
                        }
                        encryptedBytes = memoryStreamEncrypt.ToArray();
                    }
                }
            }

            return encryptedBytes;
        }

        /// <summary>
        /// Decryptes an encrypted array of bytes with the Advanced Encryption Standard and returns the result as a string.
        /// </summary>
        /// <param name="encryptedBytes">The array of bytes which will be decrypted.</param>
        /// <returns>The decrpyted array of bytes as a string</returns>
        public static string DecryptStringAes(byte[] encryptedBytes)
        {
            string output = null;

            using (Aes aesAlgorithm = Aes.Create())
            {
                aesAlgorithm.Key = GetKey();
                aesAlgorithm.IV = GetInitVector();

                ICryptoTransform decryptor = aesAlgorithm.CreateDecryptor(aesAlgorithm.Key, aesAlgorithm.IV);

                using (MemoryStream memoryStreamDecrypt = new MemoryStream(encryptedBytes))
                {
                    using (CryptoStream cryptoStreamDecrypt = new CryptoStream(memoryStreamDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReaderDecrypt = new StreamReader(cryptoStreamDecrypt))
                        {
                            output = streamReaderDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return output;
        }

        /* 
         * NOTICE
         * I know that the following two methods aren't very secure and shouldn't be used in normal circumstances (especially when everyone can read them on GitHub).
         * However, they are just here so that not everyone (or every generic program) can read the app key of this application.
         */

        /// <summary>
        /// NOT VERY SECURE (see above). Returns the init vector that is used for the Advanced Encryption Standard.
        /// The key contains the name of the user cut or expanded to a lenght of 16 characters.
        /// </summary>
        /// <returns>The init vector as an byte array</returns>
        private static byte[] GetInitVector()
        {
            string userName = RemoveUnnecessaryCharacters(Environment.UserName);

            string initVectorString = "";
            while (initVectorString.Length < 16)
            {
                initVectorString += userName;
            }
            initVectorString = initVectorString.Substring(0, 16);

            return Encoding.UTF8.GetBytes(initVectorString);
        }

        /// <summary>
        /// NOT VERY SECURE (see above). Returns the key that is used for the Advanced Encryption Standard.
        /// The key contains the name of the user's computer cut or expanded to a lenght of 24 characters.
        /// </summary>
        /// <returns>The key as an byte array</returns>
        private static byte[] GetKey()
        {
            string computerName = RemoveUnnecessaryCharacters(Environment.MachineName);

            string keyString = "";
            while (keyString.Length < 24)
            {
                keyString += computerName;
            }
            keyString = keyString.Substring(0, 24);

            return Encoding.UTF8.GetBytes(keyString);
        }

        /// <summary>
        /// Removes unwanted characters from a string. This method is used by GetInitVector and GetKey.
        /// </summary>
        /// <param name="text">The string from which the characters will be removed</param>
        /// <returns>The string without the unwanted characters</returns>
        private static string RemoveUnnecessaryCharacters(string text)
        {
            string[] charsToRemove = new string[] { " ", ";", "'", "@", ",", ".", "-" };
            foreach (string currentChar in charsToRemove)
            {
                text = text.Replace(currentChar, string.Empty);
            }

            return text;
        }
    }
}
