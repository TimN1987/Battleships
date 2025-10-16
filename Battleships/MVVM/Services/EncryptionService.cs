using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Battleships.MVVM.Factories;
using Battleships.MVVM.Model;
using Battleships.MVVM.Model.DataTransferObjects;
using Newtonsoft.Json;

namespace Battleships.MVVM.Services
{
    /// <summary>
    /// Defines a contract for encrypting and decrypting save game data. This interface is used for 
    /// preparing game data to be stored securely as an encrypted string and preparing encrypted data 
    /// to be loaded to the application once read from the database.
    /// </summary>
    public interface IEncryptionService
    {
        public Task<string?> EncryptGameData(Game inputGameData);
        public Task<GameDTO?> DecryptGameData(string encryptedJsonData);
    }

    /// <summary>
    /// A class for encrypting and decrypting game data. It provides methods for generating and storing an 
    /// encryption key, using this to convert a Game object to a JSON string and encrypting this. It also 
    /// provides methods for decrypting the JSON string and converting it back to a Game object. This class 
    /// is designed to be used as part of the game data storage and retrieval process, ensuring that all 
    /// data is secured in a way that is not easily accessible to users. It uses AES encryption to secure 
    /// the data in a way that can be easily stored and retrieved without enabling outside manipulation.
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        #region Fields
        private readonly IEventLogger _eventLogger;
        private const string EventSourceName = nameof(EncryptionService);
        private const int RetryCount = 3; // Number of retries for mutiple attempts at operations
        #endregion //Fields

        public EncryptionService(ILoggerFactory loggerFactory)
        {
            _eventLogger = loggerFactory.CreateLogger(EventSourceName) ?? throw new ArgumentNullException(nameof(loggerFactory), "Logger factory returned null.");
            Task.Run(async () => await GenerateNewKeyIfNeeded());
        }

        #region Encryption Key Methods
        /// <summary>
        /// Generates a new encryption key if one does not already exist. It ensures that the encryption key 
        /// directory exists with a valid path. If the encryption key file does not exist, it generates a new key.
        /// </summary>
        /// <returns>A boolean value indicating whether the task was successful.</returns>
        private async Task<bool> GenerateNewKeyIfNeeded()
        {
            try
            {
                var encryptionKeyFilePath = await GenerateValidEncryptionPath("encryption.key")
                                            ?? throw new InvalidOperationException("Encryption key path could not be generated.");

                if (File.Exists(encryptionKeyFilePath))
                    return true;

                for (int i = 0; i < RetryCount; i++)
                {
                    try
                    {
                        byte[] key = new byte[32];
                        RandomNumberGenerator.Fill(key);

                        string base64Key = Convert.ToBase64String(key);

                        File.WriteAllText(encryptionKeyFilePath, base64Key);
                        File.SetAttributes(encryptionKeyFilePath, FileAttributes.Hidden | FileAttributes.ReadOnly);
                    }
                    catch (Exception ex) when (i == RetryCount - 1)
                    {
                        _eventLogger.LogCritical($"Error generating encryption key after {RetryCount} attempts: {ex.Message}", ex);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        _eventLogger.LogWarning($"Error generating encryption key: {ex.Message}");
                    }

                    await Task.Delay(100 * (int)Math.Pow(2, i));
                }
            }
            catch (InvalidOperationException ex)
            {
                _eventLogger.LogCritical($"Encryption key path could not be generated: {ex.Message}", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                _eventLogger.LogCritical($"Could not access requested directory: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _eventLogger.LogCritical($"Error generating encryption key: {ex.Message}", ex);

            }

            return false;
        }

        /// <summary>
        /// Retrieves the encryption key from the file system. It attempts to read the encryption key file 
        /// by generating the appropriate file path. If the file does not exist, it will attempt to create 
        /// a new key and return this. If it is impossible to retrieve a valid encryption key, it will log 
        /// a critical error and return null. This null return will need to be handled by the calling method 
        /// to temporarily disable save game functionality until a valid key can be generated.
        /// </summary>
        /// <param name="firstAttempt"></param>
        /// <returns>A byte array containing the stored encryption key. This may be a new key if a previous 
        /// key cannot be found. If no key can be found or generated, a null value will be returned.</returns>
        internal async Task<byte[]?> RetrieveKey(bool firstAttempt = false)
        {
            try
            {
                var encryptionKeyFilePath = await GenerateValidEncryptionPath("encryption.key")
                    ?? throw new InvalidOperationException("Encryption key path could not be generated.");

                if (!File.Exists(encryptionKeyFilePath))
                    throw new FileNotFoundException("Encryption key file not found. Please generate a new key.");

                try
                {
                    string base64Key = await File.ReadAllTextAsync(encryptionKeyFilePath);

                    byte[] key = Convert.FromBase64String(base64Key);

                    return key;
                }
                catch (Exception ex)
                {
                    _eventLogger.LogCritical($"Encryption key could not be retrieved.", ex);
                }
            }
            catch (Exception ex) when (!firstAttempt)
            {
                _eventLogger.LogCritical($"No encryption key is available after final attempt: {ex.Message}", ex);
                return null;
            }
            catch (UnauthorizedAccessException ex)
            {
                _eventLogger.LogCritical($"Could not access requested directory: {ex.Message}", ex);
            }
            catch (InvalidOperationException ex)
            {
                _eventLogger.LogCritical($"Unable to retrieve encryption key: {ex.Message}", ex);
            }
            catch (FileNotFoundException ex)
            {
                _eventLogger.LogCritical($"File unexpectedly could not be found: {ex.Message}", ex);
                return await CreateAndReturnNewKeyFallback();
            }
            catch (Exception ex)
            {
                _eventLogger.LogCritical($"Unable to retrieve encryption key: {ex.Message}", ex);
            }

            return null;
        }
        #endregion //Encryption Key Methods

        #region Encryption Key Helper Methods
        /// <summary>
        /// Generates a valid encryption path for the encryption key file. This method creates a directory in 
        /// the ApplicationData folder of the current user. If the directory does not exist, it attempts to 
        /// create it. If the directory cannot be created after a specified number of retries, it logs a critical 
        /// error. It returns null if it cannot create a valid path to a valid directory.
        /// The method can be used to generate a path for the encryption key file (if a filename is given) or 
        /// just the directory path (if the <paramref name="fileName"/> parameter is null).
        /// </summary>
        /// <param name="fileName">The name of the file containing the encryption key.</param>
        /// <returns>A string representing the path to the file named <paramref name="fileName"/>. If 
        /// <paramref name="fileName"/> is null, returns the path to a directory in Application Data 
        /// where the encryption key can be stored. Returns null if the directory path cannot be created 
        /// or if the directory itself cannot be created after three attempts. This can be handled by the 
        /// calling method to avoid further issues with creating and storing a new encryption key.</returns>
        private async Task<string?> GenerateValidEncryptionPath(string? fileName = null)
        {
            string encryptionKeyDirectoryPath;

            try
            {
                encryptionKeyDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Battleships");

                if (string.IsNullOrWhiteSpace(encryptionKeyDirectoryPath))
                    throw new InvalidOperationException($"Could not create a directory path for the encryption key.");

                if (!Directory.Exists(encryptionKeyDirectoryPath))
                {
                    for (int i = 0; i < RetryCount; i++)
                    {
                        try
                        {
                            Directory.CreateDirectory(encryptionKeyDirectoryPath);

                            if (Directory.Exists(encryptionKeyDirectoryPath))
                                break;
                        }
                        catch (Exception ex) when (i == RetryCount - 1)
                        {
                            _eventLogger.LogCritical($"Error creating encryption folder after {RetryCount} attempts: {ex.Message}", ex);
                            return null;
                        }
                        catch (Exception ex)
                        {
                            _eventLogger.LogWarning($"Error creating encryption folder: {ex.Message}");
                        }

                        await Task.Delay(100 * (int)Math.Pow(2, i));
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                _eventLogger.LogCritical(ex.Message, ex);
                return null;
            }
            catch (UnauthorizedAccessException ex)
            {
                _eventLogger.LogCritical($"Could not access requested directory: {ex.Message}", ex);
                return null;
            }
            catch (Exception ex)
            {
                _eventLogger.LogCritical($"Error generating encryption path: {ex.Message}", ex);
                return null;
            }

            return (fileName is null) ? encryptionKeyDirectoryPath : Path.Combine(encryptionKeyDirectoryPath, fileName);
        }

        /// <summary>
        /// This is a fallback method for when no valid encryption key can be retrieved. It will attempt to 
        /// generate a new key stored at the expected file path and return this. If the key cannot be generated, 
        /// it will return a null value.
        /// </summary>
        /// <returns>A byte array containing a new encryption key. If no key can be generated, a null value 
        /// is returned.</returns>
        private async Task<byte[]?> CreateAndReturnNewKeyFallback()
        {
            if (await GenerateNewKeyIfNeeded())
                return await RetrieveKey();
            return null;
        }
        #endregion //Encryption Key Helper Methods

        #region Encryption Methods
        /// <summary>
        /// Encrypts the game data by converting it to a JSON string and then encrypting this string using 
        /// AES encryption. This method handles any exceptions that may occur during the conversion and 
        /// returns a null value if the encryption fails. It logs any exceptions that occur. It implements a 
        /// retry mechanism to attempt the encryption multiple times in case of transient errors.
        /// </summary>
        /// <param name="selectedGame">The Game object containing the game data to be encrypted ready for 
        /// storage in the database.</param>
        /// <returns>A string containing an encrypted Json serialization of the game data. Returns null if 
        /// the conversion and encryption cannot be completed after multiple attemps.</returns>
        public async Task<string?> EncryptGameData(Game selectedGame)
        {
            try
            {
                if (selectedGame is null)
                    throw new ArgumentException("Game data input is null.");

                for (int i = 0; i < RetryCount; i++)
                {
                    try
                    {
                        var jsonGameData = ConvertGameToJson(selectedGame)
                            ?? throw new InvalidOperationException($"Game data could not be converted to Json.");

                        return await EncryptJsonData(jsonGameData)
                            ?? throw new InvalidOperationException($"Game data could not be encrypted.");
                    }
                    catch (Exception ex) when (i == RetryCount - 1)
                    {
                        _eventLogger.LogCritical($"Could not successfully create encrypted data after {RetryCount} attempts: {ex.Message}", ex);
                    }
                    catch (InvalidOperationException ex)
                    {
                        _eventLogger.LogWarning($"Error encrypting game data: {ex.Message}. Attempt {i + 1} of {RetryCount}.");
                    }
                    catch (Exception ex)
                    {
                        _eventLogger.LogWarning($"Error encrypting game data: {ex.Message}. Attempt {i + 1} of {RetryCount}.");
                    }

                    await Task.Delay(i * (int)Math.Pow(2, i));
                }
            }
            catch (ArgumentException ex)
            {
                _eventLogger.LogCritical($"Game data input is null: {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// Converts the game data to a JSON string. This method uses the Newtonsoft.Json library to 
        /// serialize the Game object to a JSON string. It handles any exceptions that may occur during 
        /// and returns null if the conversion fails. This method is used to prepare the game data for 
        /// encryption. Logs any exceptions that occur and returns null if the encryption cannot be completed.
        /// </summary>
        /// <param name="selectedGame">A Game object representing game data to be encrypted and saved.</param>
        /// <returns>A string containing a Json representation of the Game data. Returns null if the Json 
        /// cannot be created succesfully. Returns null if the encryption cannot be successfully completed.</returns>
        internal string? ConvertGameToJson(Game selectedGame)
        {
            try
            {
                if (selectedGame is null)
                    throw new ArgumentException("Game data input is null.");

                var gameDTO = selectedGame.GetDTO();

                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.All
                };

                return JsonConvert.SerializeObject(
                    gameDTO,
                    settings
                );
            }
            catch (Exception ex)
            {
                _eventLogger.LogCritical($"Error converting game data to Json: {ex.Message}", ex);
                return null;
            }
        }

        /// <summary>
        /// Encrypts a string using Aes encryption in preparation for storage in the database. This protects 
        /// the game data against user manipulation.
        /// </summary>
        /// <param name="jsonGameData">A string contraining a copy of the game data serialized as a Json.</param>
        /// <returns>A string containing the encrypted Json string.</returns>
        internal async Task<string?> EncryptJsonData(string jsonGameData)
        {
            try
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(jsonGameData, nameof(jsonGameData));

                byte[] key = await RetrieveKey()
                    ?? throw new InvalidOperationException("Encryption key could not be retrieved.");

                byte[] iv = new byte[16];
                RandomNumberGenerator.Fill(iv);

                using var aes = Aes.Create();
                aes.KeySize = 256;
                aes.Key = key;
                aes.IV = iv;
                aes.Padding = PaddingMode.PKCS7;

                //Store initialization vector in the first 16 bytes of the encrypted data
                using var memoryStream = new MemoryStream();
                memoryStream.Write(iv, 0, iv.Length);

                //Write the encrypted data to the stream
                using var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);

                //Ensure utf8 encoding without BOM
                var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                using var streamWriter = new StreamWriter(cryptoStream, utf8NoBom);
                await streamWriter.WriteAsync(jsonGameData);
                await streamWriter.FlushAsync();
                cryptoStream.Clear();

                return Convert.ToBase64String(memoryStream.ToArray());
            }
            catch (ArgumentException ex)
            {
                _eventLogger.LogCritical($"Game data input is null: {ex.Message}", ex);
            }
            catch (InvalidOperationException ex)
            {
                _eventLogger.LogCritical($"Encryption key could not be retrieved: {ex.Message}", ex);
            }
            catch (CryptographicException ex)
            {
                _eventLogger.LogCritical($"Error encrypting game data: {ex.Message}", ex);
            }
            catch (IOException ex)
            {
                _eventLogger.LogCritical($"Error writing game data to stream: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _eventLogger.LogCritical($"Error encrypting game data: {ex.Message}", ex);
            }

            return null;
        }
        #endregion //Encryption Methods

        #region Decryption methods
        /// <summary>
        /// Decrypts the game data by converting the encrypted string back to a JSON string and then 
        /// deserializing the Json string to a Game object. This method handles any exceptions that may 
        /// occur during the conversion and deserialization and returns a null value if the decryption fails. 
        /// This method enables the game data to be loaded from the database and used in the application. If a 
        /// null value is returned, the calling class will need to handle this to avoid further issues with 
        /// game loading functionality.
        /// </summary>
        /// <param name="encryptedData">A string containing an AES encryption of a JSon serialization of 
        /// a Game object.</param>
        /// <returns>A Game object containing game data loaded from the database. Returns null if the 
        /// encrypted data could not be decrypted or the Json could not be deserialized.</returns>
        public async Task<GameDTO?> DecryptGameData(string encryptedData)
        {
            try
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(encryptedData, nameof(encryptedData));

                for (int i = 0; i < RetryCount; i++)
                {
                    try
                    {
                        var jsonGameData = await DecryptJsonData(encryptedData)
                            ?? throw new InvalidOperationException("Game data could not be decrypted.");

                        var gameData = DeserializeGameData(jsonGameData)
                            ?? throw new InvalidOperationException("Game data could not be deserialized.");

                        return gameData;
                    }
                    catch (Exception ex) when (i == RetryCount - 1)
                    {
                        _eventLogger.LogCritical($"Could not successfully decrypt data after {RetryCount} attempts: {ex.Message}", ex);
                    }
                    catch (InvalidOperationException ex)
                    {
                        _eventLogger.LogWarning($"Error decrypting game data: {ex.Message}. Attempt {i + 1} of {RetryCount}.");
                    }
                    catch (Exception ex)
                    {
                        _eventLogger.LogWarning($"Error decrypting game data: {ex.Message}. Attempt {i + 1} of {RetryCount}.");
                    }

                    await Task.Delay(i * (int)Math.Pow(2, i));
                }
            }
            catch (ArgumentException ex)
            {
                _eventLogger.LogCritical($"Game data input is null: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _eventLogger.LogCritical($"Error decrypting game data: {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// Decrypts a string containing an encrypted Json string. This method uses the AES encryption 
        /// key to decrypt the data. It handles any exceptions that may occur during the decryption process 
        /// and returns a null value if the decryption fails. This method is used as the first part of decrypting 
        /// the game data to be loaded from the database before deserialization. It implements a retry mechanism 
        /// to attempt the decryption mutliple times in the case of failure. Returns null if the decryption 
        /// cannot be completed successfully to allow the calling class to handle this.
        /// </summary>
        /// <param name="encryptedJsonData">A string containing AES encrypted JSon data read from the 
        /// save games database.</param>
        /// <returns>A string containing the Json serialization of the Game object read from the database. 
        /// Returns null if the data cannot successfully be decrypted.</returns>
        internal async Task<string?> DecryptJsonData(string encryptedJsonData)
        {
            try
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(encryptedJsonData, nameof(encryptedJsonData));

                for (int i = 0; i < RetryCount; i++)
                {
                    try
                    {
                        byte[] encryptedBytes = Convert.FromBase64String(encryptedJsonData);

                        byte[] key = await RetrieveKey()
                            ?? throw new FileNotFoundException("Encryption key could not be retrieved.");

                        if (encryptedBytes.Length < 16)
                            throw new InvalidOperationException("The encrypted data is too small to contain valid data.");

                        using var aes = Aes.Create()
                            ?? throw new InvalidOperationException("Unable to create AES instance.");

                        byte[] iv = new byte[16];
                        Array.Copy(encryptedBytes, 0, iv, 0, iv.Length);

                        aes.KeySize = 256;
                        aes.Key = key;
                        aes.IV = iv;
                        aes.Padding = PaddingMode.PKCS7;

                        using var memoryStream = new MemoryStream(encryptedBytes, iv.Length, encryptedBytes.Length - iv.Length);
                        using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);

                        //UTF8 decoding without BOM
                        var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                        using var streamReader = new StreamReader(cryptoStream, utf8NoBom);

                        var decryptedData = await streamReader.ReadToEndAsync()
                            ?? throw new InvalidOperationException("Decrypted data is null.");

                        return decryptedData;
                    }
                    catch (Exception ex) when (i == RetryCount - 1)
                    {
                        _eventLogger.LogCritical($"Could not successfully decrypt data after {RetryCount} attempts: {ex.Message}", ex);
                    }
                    catch (FileNotFoundException ex)
                    {
                        _eventLogger.LogWarning($"Encryption key could not be retrieved: {ex.Message}");
                    }
                    catch (InvalidOperationException ex)
                    {
                        _eventLogger.LogWarning($"Error decrypting game data: {ex.Message}. Attempt {i + 1} of {RetryCount}.");
                    }
                    catch (Exception ex)
                    {
                        _eventLogger.LogWarning($"Error decrypting game data: {ex.Message}. Attempt {i + 1} of {RetryCount}.");
                    }

                    await Task.Delay(i * (int)Math.Pow(2, i));
                }
            }
            catch (ArgumentException ex)
            {
                _eventLogger.LogCritical($"Game data input is null: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _eventLogger.LogCritical($"Error decrypting game data: {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// Deserializes a string containing a Json serialization of a Game object. This method uses the 
        /// NewtonSoft.Json library to deserialize the string back to a Game object. It handles any exceptions 
        /// and returns null if the deserialization fails. This method is used as the second part of decrypting 
        /// the game data to be loaded from the database. It implements a retry mechanism to attempt the 
        /// operation multiple times in the case of failure before returning null.
        /// </summary>
        /// <param name="jsonGameData">A string containing a Json serialization of the Game object containing 
        /// the loaded game data.</param>
        /// <returns>A Game object containing the game data for the requested load game. Returns null if the 
        /// Json string cannot be deserialized successfully.</returns>
        internal GameDTO? DeserializeGameData(string jsonGameData)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jsonGameData))
                    throw new ArgumentException("Json data input is null.");

                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.All
                };

                return JsonConvert.DeserializeObject<GameDTO>(jsonGameData, settings);
            }
            catch (Exception ex)
            {
                _eventLogger.LogCritical($"Error deserializing game data: {ex.Message}", ex);
                return null;
            }
        }

        #endregion //Decryption methods
    }
}
