using System.Security.Cryptography;

namespace MyShowBook.Api.Utility;

public static class PasswordHasher
{
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
