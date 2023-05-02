using RpgCollector.Models.AccountModel;
using System.Security.Cryptography;

namespace RpgCollector.Utility;

public static class HashManager
{
    private const string secretKey = "helloworld_im_iron_man";

    public static void SetSalt(this User user)
    {
        var salt = GenerateSalt();
        user.PasswordSalt = Convert.ToBase64String(salt);
    }

    public static void SetHash(this User user)
    {
        user.Password = GenerateHash(user.Password, user.PasswordSalt);
    }

    public static byte[] GenerateSalt()
    {
        var rng = RandomNumberGenerator.Create();
        var salt = new byte[24];
        rng.GetBytes(salt);

        return salt;
    }

    public static string GenerateHash(string password, string saltString)
    {
        var salt = Convert.FromBase64String(saltString);
#pragma warning disable SYSLIB0041
        using var hashGenerator = new Rfc2898DeriveBytes(password, salt: salt, 1000);
#pragma warning restore SYSLIB0041
        hashGenerator.IterationCount = 10101;
        var bytes = hashGenerator.GetBytes(24);
        return Convert.ToBase64String(bytes);
    }

    public static string GenerateAuthToken()
    {
        var rng = RandomNumberGenerator.Create();
        var salt = new byte[24];
        rng.GetBytes(salt);

#pragma warning disable SYSLIB0041
        using var hashGenerator = new Rfc2898DeriveBytes(secretKey, salt: salt, 1000);
#pragma warning restore SYSLIB0041
        hashGenerator.IterationCount = 10101;
        var bytes = hashGenerator.GetBytes(24);
        return Convert.ToBase64String(bytes);
    }
}
