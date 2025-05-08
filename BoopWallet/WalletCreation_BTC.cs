using System;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using NBitcoin;

using static PasswordHasher;
using Avalonia.Platform.Storage;

public class WalletCreation_BTC
{
    public class PrivateKeyData
    {
        public string? PrivateKey { get; set; }
        public string? PrivateKeySalt { get; set; }
    }

    public class RecoveryPhraseData
    {
        public string? SomeRecoveryPhrase { get; set; }
        public string? RecoveryPhraseSalt { get; set; }
    }

    public class WalletData
    {
        public string? PublicKey { get; set; }
        public PrivateKeyData? PrivateKey { get; set; }
        public RecoveryPhraseData? RecoveryPhrase { get; set; }
    }
    //creating and saving the wallet
    public static void SaveWalletToFile(
       string? currency,
       string? publicKey,
       string? privateKey,
       string? privateKeySalt,
       string? recoveryPhrase,
       string? recoveryPhraseSalt,
       string? outputDirectory = null // optional: allow custom directory
   )
    {
        // Build wallet data
        var data = new Dictionary<string, WalletData>
        {
            [currency] = new WalletData
            {
                PublicKey = publicKey,
                PrivateKey = new PrivateKeyData
                {
                    PrivateKey = privateKey,
                    PrivateKeySalt = privateKeySalt
                },
                RecoveryPhrase = new RecoveryPhraseData
                {
                    SomeRecoveryPhrase = recoveryPhrase,
                    RecoveryPhraseSalt = recoveryPhraseSalt
                }
            }
        };
        string jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

        // Set directory (default: current)
        string directory = string.IsNullOrWhiteSpace(outputDirectory)
            ? Directory.GetCurrentDirectory()
            : outputDirectory;

        // Ensure directory exists
        Directory.CreateDirectory(directory);

        // Determine next available file name
        string baseFileName = $"wallet_{currency}_";
        var existingFiles = Directory.GetFiles(directory, $"{baseFileName}*.json");

        int maxIndex = 0;
        var regex = new Regex($@"{baseFileName}(\d+)\.json");

        foreach (var file in existingFiles)
        {
            var match = regex.Match(Path.GetFileName(file));
            if (match.Success && int.TryParse(match.Groups[1].Value, out int index))
            {
                if (index > maxIndex)
                    maxIndex = index;
            }
        }

        int nextIndex = maxIndex + 1;
        string finalFileName = Path.Combine(directory, $"{baseFileName}{nextIndex}.json");

        // Save JSON
        File.WriteAllText(finalFileName, jsonString, Encoding.UTF8);
        Console.WriteLine($"Saved wallet data to: {finalFileName}");
    }

//public key only wallet
    public static void PublicKeyOnlyWallet_BTC(string publicKey)
    {
            SaveWalletToFile(
                currency: "BTC",
                publicKey: publicKey,
                privateKey: null,
                privateKeySalt: null,
                recoveryPhrase: null,
                recoveryPhraseSalt: null
        );
    }
    //private key only wallet
    public static void PrivateKeyOnlyWallet_BTC(string privateKey, string password)
    {
        string[] passwordFile = File.ReadAllLines("password.txt");

        string passwordHash = passwordFile[0];
        string passwordSalt = passwordFile[1];

        byte[] saltBytes = Convert.FromBase64String(passwordSalt);

        string enteredPasswordHash = HashPassword(password, salt: saltBytes).Item1;

        if (passwordHash == enteredPasswordHash)
        {
            var privateKeyHashingResults = HashPassword(privateKey);
            string privateKeyHash = privateKeyHashingResults.Item1;
            string privateKeyHashSalt = privateKeyHashingResults.Item2;

            SaveWalletToFile(
                currency: "BTC",
                publicKey: null,
                privateKey: privateKeyHash,
                privateKeySalt: privateKeyHashSalt,
                recoveryPhrase: null,
                recoveryPhraseSalt: null
            );

            // make a wallet using the recovery phrase
            // this is going to be very complicated cause im thinking we change the wallet saving architecture and in file we save the
            // private key in all formats (taproot, legacy,) (encrypted of course) so that the user later while managing the wallet (making transactions)
            // (recieving etc) has the option to view his main (0/0/0/0/0) wallet adress but also can generate new ones (0/0/0/0/0/1)(also in all formats...)
        }
    }

    //probably complete AI slop but kinda works
    public static bool DoesKeyMatch(string privKeyInput, string publicKeyInput)
    {
        if (string.IsNullOrWhiteSpace(privKeyInput) || string.IsNullOrWhiteSpace(publicKeyInput))
            throw new ArgumentException("Private key and public key must be non-empty strings.");

        byte[] pubKeyBytes;

        try
        {
            // Detect and handle the format of the provided public key
            pubKeyBytes = ParsePublicKey(publicKeyInput);
        }
        catch
        {
            throw new ArgumentException("Provided public key is not in a valid format.");
        }

        Key key;

        // Case 1: WIF input (legacy/SegWit)
        if (privKeyInput.StartsWith("5") || privKeyInput.StartsWith("K") || privKeyInput.StartsWith("L"))
        {
            try
            {
                key = Key.Parse(privKeyInput, Network.Main);
            }
            catch
            {
                throw new ArgumentException("Invalid WIF private key.");
            }
        }
        // Case 2: raw hex private key
        else if (privKeyInput.Length == 64)
        {
            try
            {
                byte[] privKeyBytes = Convert.FromHexString(privKeyInput);
                key = new Key(privKeyBytes);
            }
            catch
            {
                throw new ArgumentException("Private key is not valid hex.");
            }
        }
        else
        {
            throw new ArgumentException("Unsupported private key format.");
        }

        // Compare based on public key length/type
        if (pubKeyBytes.Length == 65) // uncompressed
        {
            bool isMatch = key.PubKey.Decompress().ToBytes().SequenceEqual(pubKeyBytes);
            return isMatch;
        }
        else if (pubKeyBytes.Length == 33) // compressed
        {
            bool isMatch = key.PubKey.Compress().ToBytes().SequenceEqual(pubKeyBytes);
            return isMatch;
        }
        else if (pubKeyBytes.Length == 32) // x-only Taproot pubkey
        {
            byte[] xOnlyPubKey = key.PubKey.ToBytes().Skip(1).ToArray(); // remove prefix (02/03)
            bool isMatch = xOnlyPubKey.SequenceEqual(pubKeyBytes);
            return isMatch;
        }
        else
        {
            throw new ArgumentException("Unrecognized public key format.");
        }
    }

    // Helper method to parse the public key input in different formats
    private static byte[] ParsePublicKey(string publicKeyInput)
    {
        // Try base58 decoding first (common for WIF keys)
        try
        {
            // If it's base58, decode it into a byte array
            if (IsBase58(publicKeyInput))
            {
                return Base58Decode(publicKeyInput);
            }
        }
        catch { /* ignore, try next formats */ }

        // Try base64 decoding
        try
        {
            if (IsBase64(publicKeyInput))
            {
                return Convert.FromBase64String(publicKeyInput);
            }
        }
        catch { /* ignore, try next formats */ }

        // Try hex string decoding
        try
        {
            return Convert.FromHexString(publicKeyInput);
        }
        catch { /* ignore, proceed to next */ }

        // Try raw byte array decoding if it's a string representation
        try
        {
            return Encoding.UTF8.GetBytes(publicKeyInput); // for cases where it's passed as a non-standard format
        }
        catch
        {
            throw new ArgumentException("Unrecognized public key format.");
        }
    }

    // Helper method to check if the string is Base58 encoded
    private static bool IsBase58(string input)
    {
        // A quick check for base58 characters
        const string base58Chars = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        return input.All(c => base58Chars.Contains(c));
    }

    // Helper method to check if the string is Base64 encoded
    private static bool IsBase64(string input)
    {
        try
        {
            Convert.FromBase64String(input);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Custom Base58 decoder function
    private static byte[] Base58Decode(string input)
    {
        // Base58 alphabet
        const string base58Chars = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        int length = input.Length;

        // Prepare the result array
        byte[] result = new byte[length];

        // Decode Base58 into a byte array
        for (int i = 0; i < length; i++)
        {
            int digit = base58Chars.IndexOf(input[i]);
            if (digit == -1) throw new ArgumentException("Invalid Base58 character");

            for (int j = result.Length - 1; j >= 0; j--)
            {
                digit += result[j] * 58;
                result[j] = (byte)(digit % 256);
                digit /= 256;
            }
        }

        // Strip leading zero bytes
        int leadingZeroCount = 0;
        while (leadingZeroCount < length && result[leadingZeroCount] == 0)
        {
            leadingZeroCount++;
        }

        return result.Skip(leadingZeroCount).ToArray();
    }
    //

    public static void PrivateKeyAndPublicKeyWallet_BTC(string publicKey, string privateKey, string password)
    {
        string[] passwordFile = File.ReadAllLines("password.txt");

        string passwordHash = passwordFile[0];
        string passwordSalt = passwordFile[1];

        byte[] saltBytes = Convert.FromBase64String(passwordSalt);

        string enteredPasswordHash = HashPassword(password, salt: saltBytes).Item1;

        if (passwordHash == enteredPasswordHash)
        {
            var privateKeyHashingResults = HashPassword(privateKey);
            string privateKeyHash = privateKeyHashingResults.Item1;
            string privateKeyHashSalt = privateKeyHashingResults.Item2;

            if (DoesKeyMatch(privateKey, publicKey) == true)
            {
                SaveWalletToFile(
                    currency: "BTC",
                    publicKey: publicKey,
                    privateKey: privateKeyHash,
                    privateKeySalt: privateKeyHashSalt,
                    recoveryPhrase: null,
                    recoveryPhraseSalt: null
                );
            }
            else
            {
                Console.WriteLine("The provided private key does not match the public key.");
            }
        }
    }
    //full wallet
    public static void FullWallet_BTC(string mnemonic,string password, string? privateKey = null, string? publicKey = null) //public and private key are optional here
    {
    }
}