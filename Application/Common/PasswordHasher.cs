using System;
using System.Security.Cryptography;

namespace Application.Common;

public static class PasswordHasher
{
    // PBKDF2 implementation
    public static string HashPassword(string password)
    {
        var salt = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // PBKDF2 using Rfc2898DeriveBytes
        using var deriveBytes = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
        var subkey = deriveBytes.GetBytes(32);

        var outputBytes = new byte[1 + salt.Length + subkey.Length];
        outputBytes[0] = 0x01; // format marker
        Buffer.BlockCopy(salt, 0, outputBytes, 1, salt.Length);
        Buffer.BlockCopy(subkey, 0, outputBytes, 1 + salt.Length, subkey.Length);
        return Convert.ToBase64String(outputBytes);
    }

    public static bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword)) return false;
        var decoded = Convert.FromBase64String(hashedPassword);
        if (decoded.Length < 1) return false;
        var format = decoded[0];
        if (format != 0x01) return false;

        var salt = new byte[16];
        Buffer.BlockCopy(decoded, 1, salt, 0, salt.Length);
        var storedSubkey = new byte[32];
        Buffer.BlockCopy(decoded, 1 + salt.Length, storedSubkey, 0, storedSubkey.Length);

        using var deriveBytes = new Rfc2898DeriveBytes(providedPassword, salt, 100_000, HashAlgorithmName.SHA256);
        var generatedSubkey = deriveBytes.GetBytes(32);

        return CryptographicOperations.FixedTimeEquals(storedSubkey, generatedSubkey);
    }
}
