using System.Security.Cryptography;

namespace MyShowBook.Api.Utility;

public static class PasswordHasher
{
    private const int Iterations = 100_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        return "PBKDF2$SHA256$" + Iterations + "$" +
               Convert.ToBase64String(salt) + "$" +
               Convert.ToBase64String(hash);
    }

    public static bool Verify(string password, string storedHash)
    {
        var parts = storedHash.Split('$');

        if (parts.Length != 5 ||
            parts[0] != "PBKDF2" ||
            parts[1] != "SHA256" ||
            !int.TryParse(parts[2], out var iterations))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[3]);
            var expected = Convert.FromBase64String(parts[4]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);

            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}