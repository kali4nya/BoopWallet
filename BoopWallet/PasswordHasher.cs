using System;
using System.Text;
using Konscious.Security.Cryptography;

public class PasswordHasher
{
    public static Tuple<string, string> HashPassword(string password, int timeCost = 4, int memoryCost = 65536, int parallelism = 1, int hashLength = 64, byte[] salt = null)
    {
        if (salt == null)
        {
            salt = new byte[16];
            new Random().NextBytes(salt); // Generate a random salt
        }
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

        //defining
        var argon2 = new Argon2id(passwordBytes);
        argon2.DegreeOfParallelism = parallelism;
        argon2.MemorySize = memoryCost;
        argon2.Iterations = timeCost;
        argon2.Salt = salt;

        //hashing
        var hashedPassword = argon2.GetBytes(hashLength);

        argon2.Reset();
        argon2.Dispose();

        return Tuple.Create(Convert.ToBase64String(hashedPassword), Convert.ToBase64String(salt));
    }
}