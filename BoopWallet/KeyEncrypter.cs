using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class KeyEncrypter
{
    public static (byte[] cipherText, byte[] salt, byte[] iv, byte[] hmac) EncryptPrivateKey(string privateKey, string password, byte[] salt = null)
    {
        if (string.IsNullOrEmpty(privateKey)) throw new ArgumentException("Private key is required.");
        if (string.IsNullOrEmpty(password)) throw new ArgumentException("Password is required.");

        // Generate random salt if not provided
        if (salt == null)
        {
            salt = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
        }

        using var keyDeriver = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
        byte[] key = keyDeriver.GetBytes(32); // AES-256

        // Generate random IV
        byte[] iv = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
            rng.GetBytes(iv);

        // Encrypt
        byte[] cipherText;
        using (var aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.IV = iv;

            using var encryptor = aes.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(privateKey);
            cipherText = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        }

        // HMAC
        using var hmacSha256 = new HMACSHA256(key);
        byte[] hmac = hmacSha256.ComputeHash(CombineArrays(iv, cipherText));

        return (cipherText, salt, iv, hmac);
    }

    public static string DecryptPrivateKey(byte[] cipherText, string password, byte[] salt, byte[] iv, byte[] hmac)
    {
        if (cipherText == null || salt == null || iv == null || hmac == null)
            throw new ArgumentException("All components are required for decryption.");

        using var keyDeriver = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
        byte[] key = keyDeriver.GetBytes(32);

        // Verify HMAC first
        using var hmacSha256 = new HMACSHA256(key);
        byte[] computedHmac = hmacSha256.ComputeHash(CombineArrays(iv, cipherText));

        if (!CompareBytes(computedHmac, hmac))
            throw new CryptographicException("HMAC validation failed. Data is corrupted or password is wrong.");

        // Decrypt
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        byte[] plainBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }

    private static byte[] CombineArrays(params byte[][] arrays)
    {
        using var ms = new MemoryStream();
        foreach (var arr in arrays)
            ms.Write(arr, 0, arr.Length);
        return ms.ToArray();
    }

    private static bool CompareBytes(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) return false;
        int result = 0;
        for (int i = 0; i < a.Length; i++)
            result |= a[i] ^ b[i];
        return result == 0; // Prevents timing attacks
    }
}
