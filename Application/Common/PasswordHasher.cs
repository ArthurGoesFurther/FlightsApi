using System;
using System.Security.Cryptography;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Application.Common;

public static class PasswordHasher
{
    // Use BCrypt for password hashing (work factor 12)
    public static string HashPassword(string password)
    {
        return BCryptNet.HashPassword(password, BCryptNet.GenerateSalt(12));
    }

    public static bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword)) return false;
        return BCryptNet.Verify(providedPassword, hashedPassword);
    }
}
