using System.Security.Cryptography;

namespace PlaceTrack.Services;

// Passwords are never saved as plain text.
// We add a random "salt", run PBKDF2 (100,000 rounds) and save: salt.hash
public static class PasswordService
{
    private const int Iterations = 100_000;

    public static string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
        return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
    }

    public static bool Verify(string password, string stored)
    {
        var parts = (stored ?? "").Split('.');
        if (parts.Length != 2) return false;

        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] expected = Convert.FromBase64String(parts[1]);
        byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);

        // Fixed-time compare, so attackers cannot guess by measuring time.
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
