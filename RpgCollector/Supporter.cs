using RpgCollector.Models;
using System.Security.Cryptography;

namespace RpgCollector
{
    public static class TimeSupporter
    {
        public static DateTime GetDateTime(this long timestamp)
        {
            DateTime currentTime = new DateTime(); // UTC 기준의 1970년 1월 1일 0시 0분 0초를 나타내는 DateTime 객체 생성
            currentTime = currentTime.AddMilliseconds(timestamp); // Timestamp 값을 더하여 현재 시간을 구함
            return currentTime;
        }

        public static bool IsTimeOut(this long timestamp)
        {
            long waitTimeout = 3600000;
            long nowTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            // 활동 기록이 waitTimeout보다 길다면 세션활동이 없는것 
            if(nowTimestamp - timestamp > waitTimeout)
            {
                return true; 
            }
            return false;
        }

        public static long GetTimeStamp()
        {
            long nowTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            return nowTimestamp;
        }
    }

    public static class AuthenticationSupporter
    {
        private const string secretKey = "helloworld_im_iron_man";
        public static void SetupSaltAndHash(this User user)
        {
            var salt = GenerateSalt();
            user.PasswordSalt = Convert.ToBase64String(salt);
            user.Password = GenerateHash(user.Password, user.PasswordSalt);
        }

        public static byte[] GenerateSalt()
        {
            var rng = RandomNumberGenerator.Create();
            var salt = new Byte[24];
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
            var salt = new Byte[24];
            rng.GetBytes(salt);

#pragma warning disable SYSLIB0041
            using var hashGenerator = new Rfc2898DeriveBytes(secretKey, salt: salt, 1000);
#pragma warning restore SYSLIB0041
            hashGenerator.IterationCount = 10101;
            var bytes = hashGenerator.GetBytes(24);
            return Convert.ToBase64String(bytes);
        }
    }
}
