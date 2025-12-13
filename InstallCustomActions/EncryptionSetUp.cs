using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace InstallCustomActions
{
    public static class EncryptionSetUp
    {
        private const string EventSourceName = "EncryptionSetUp";

        public static void GenerateEncryptionKey()
        {
            string appDataPath = GenerateEncryptionPath();

            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            string keyFilePath = GenerateEncryptionPath("encryption.key");

            if (File.Exists(keyFilePath))
                return;
            try
            {
                byte[] key = new byte[32];

                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(key);
                }

                string base64Key = Convert.ToBase64String(key);

                File.WriteAllText(keyFilePath, base64Key);
                File.SetAttributes(keyFilePath, FileAttributes.Hidden | FileAttributes.ReadOnly);

            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(EventSourceName, $"New encryption key could not be generated: {ex.Message}");
            }
        }

        private static string GenerateEncryptionPath(string fileName = null)
        {
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Battleships");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            return (fileName is null) ? folderPath : Path.Combine(folderPath, fileName);
        }
    }
}
