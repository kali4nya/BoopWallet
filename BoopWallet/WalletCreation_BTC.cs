using System;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using NBitcoin;

using static PasswordHasher;
using static KeyEncrypter;
using Avalonia.Platform.Storage;
using NBitcoin.DataEncoders;

public class WalletCreation_BTC
{
    public class PrivateKeyData
    {
        public string? PrivateKey { get; set; }
        public string? PrivateKeySalt { get; set; }
        public string? PrivateKeyIV { get; set; }
        public string? PrivateKeyHmac { get; set; }
    }

    public class RecoveryPhraseData
    {
        public string? SomeRecoveryPhrase { get; set; }
        public string? RecoveryPhraseSalt { get; set; }
        public string? RecoveryPhraseIV { get; set; }
        public string? RecoveryPhraseHmac { get; set; }
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
    string? privateKeyIV,
    string? privateKeyHmac,
    string? recoveryPhrase,
    string? recoveryPhraseSalt,
    string? recoveryPhraseIV,
    string? recoveryPhraseHmac,
    string? outputDirectory = null // optional: allow custom directory
)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be null or whitespace.", nameof(currency));

        var data = new Dictionary<string, WalletData>
        {
            [currency] = new WalletData
            {
                PublicKey = publicKey,
                PrivateKey = new PrivateKeyData
                {
                    PrivateKey = privateKey,
                    PrivateKeySalt = privateKeySalt,
                    PrivateKeyIV = privateKeyIV,
                    PrivateKeyHmac = privateKeyHmac
                },
                RecoveryPhrase = new RecoveryPhraseData
                {
                    SomeRecoveryPhrase = recoveryPhrase,
                    RecoveryPhraseSalt = recoveryPhraseSalt,
                    RecoveryPhraseIV = recoveryPhraseIV,
                    RecoveryPhraseHmac = recoveryPhraseHmac
                }
            }
        };

        string jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

        string directory = string.IsNullOrWhiteSpace(outputDirectory)
            ? Directory.GetCurrentDirectory()
            : outputDirectory;

        Directory.CreateDirectory(directory);

        string baseFileName = $"wallet_{currency}_";
        var existingFiles = Directory.GetFiles(directory, $"{baseFileName}*.json");

        int maxIndex = 0;
        var regex = new Regex($@"{Regex.Escape(baseFileName)}(\d+)\.json");

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
                privateKeyIV: null,
                privateKeyHmac: null,
                recoveryPhrase: null,
                recoveryPhraseSalt: null,
                recoveryPhraseIV: null,
                recoveryPhraseHmac: null
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
            var encrypted = KeyEncrypter.EncryptPrivateKey(privateKey, password);

            SaveWalletToFile(
                currency: "BTC",
                publicKey: null,
                privateKey: Convert.ToBase64String(encrypted.cipherText),
                privateKeySalt: Convert.ToBase64String(encrypted.salt),
                privateKeyIV: Convert.ToBase64String(encrypted.iv),
                privateKeyHmac: Convert.ToBase64String(encrypted.hmac),
                recoveryPhrase: null,
                recoveryPhraseSalt: null,
                recoveryPhraseIV: null,
                recoveryPhraseHmac: null
            );

            // make a wallet using the recovery phrase
            // this is going to be very complicated cause im thinking we change the wallet saving architecture and in file we save the
            // private key in all formats (taproot, legacy,) (encrypted of course) so that the user later while managing the wallet (making transactions)
            // (recieving etc) has the option to view his main (0/0/0/0/0) wallet adress but also can generate new ones (0/0/0/0/0/1)(also in all formats...)
        }
    }

    //probably complete AI slop but kinda works
    public static bool DoesKeyMatch(string privKeyInput, string publicKeyOrAddressInput, Network network = null)
    {
        if (string.IsNullOrWhiteSpace(privKeyInput) || string.IsNullOrWhiteSpace(publicKeyOrAddressInput))
            throw new ArgumentException("Inputs must be non-empty.");

        network ??= Network.Main;

        Key key = ParsePrivateKey(privKeyInput, network);

        // Check if the input is a valid Bitcoin address
        try
        {
            var address = BitcoinAddress.Create(publicKeyOrAddressInput, network);
            var derivedAddress = key.PubKey.GetAddress(ScriptPubKeyType.Legacy, network);
            return derivedAddress.ToString() == address.ToString();
        }
        catch
        {
            // Not an address, so fall back to public key parsing
        }

        // Otherwise, assume it's a raw public key and try to parse/compare
        var pubKeyBytes = ParsePublicKey(publicKeyOrAddressInput);

        return pubKeyBytes.Length switch
        {
            33 => key.PubKey.Compress().ToBytes().SequenceEqual(pubKeyBytes),
            65 => key.PubKey.Decompress().ToBytes().SequenceEqual(pubKeyBytes),
            32 => key.PubKey.Compress().ToBytes()[1..].SequenceEqual(pubKeyBytes), // x-only pubkey
            _ => throw new ArgumentException("Unrecognized public key format (expected 32/33/65 bytes).")
        };
    }

    private static Key ParsePrivateKey(string privKeyInput, Network network)
    {
        try
        {
            return Key.Parse(privKeyInput, network);
        }
        catch
        {
            if (IsValidHex(privKeyInput) && privKeyInput.Length == 64)
            {
                try
                {
                    byte[] privKeyBytes = Convert.FromHexString(privKeyInput);
                    return new Key(privKeyBytes);
                }
                catch
                {
                    throw new ArgumentException("Invalid hex private key.");
                }
            }

            throw new ArgumentException("Private key is neither valid WIF nor raw hex.");
        }
    }

    private static byte[] ParsePublicKey(string input)
    {
        // Try hex
        if (IsValidHex(input))
        {
            var bytes = Convert.FromHexString(input);
            if (bytes.Length is 33 or 65 or 32)
                return bytes;
        }

        // Try base64
        if (IsValidBase64(input))
        {
            try
            {
                var bytes = Convert.FromBase64String(input);
                if (bytes.Length is 33 or 65 or 32)
                    return bytes;
            }
            catch { }
        }

        // Try base58
        if (IsLikelyBase58(input))
        {
            try
            {
                var decoded = Encoders.Base58.DecodeData(input);
                if (decoded.Length is 33 or 65 or 32)
                    return decoded;
            }
            catch { }
        }

        throw new ArgumentException("Public key format not recognized or invalid.");
    }

    private static bool IsValidHex(string input) =>
        input.Length % 2 == 0 && input.All(Uri.IsHexDigit);

    private static bool IsValidBase64(string input)
    {
        Span<byte> buffer = new Span<byte>(new byte[input.Length]);
        return Convert.TryFromBase64String(input, buffer, out _);
    }

    private static bool IsLikelyBase58(string input)
    {
        const string base58Chars = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        return input.All(c => base58Chars.Contains(c));
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
            var encrypted = KeyEncrypter.EncryptPrivateKey(privateKey, password);


            if (DoesKeyMatch(privateKey, publicKey) == true)
            {
                SaveWalletToFile(
                    currency: "BTC",
                    publicKey: publicKey,
                    privateKey: Convert.ToBase64String(encrypted.cipherText),
                    privateKeySalt: Convert.ToBase64String(encrypted.salt),
                    privateKeyIV: Convert.ToBase64String(encrypted.iv),
                    privateKeyHmac: Convert.ToBase64String(encrypted.hmac),
                    recoveryPhrase: null,
                    recoveryPhraseSalt: null,
                    recoveryPhraseIV: null,
                    recoveryPhraseHmac: null
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