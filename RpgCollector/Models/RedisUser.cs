namespace RpgCollector.Models
{
    public enum UserState
    {
        Default = 0,
        Login = 1,
        Matching = 2,
        Playing = 3
    }

    public class RedisUser
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string AuthToken { get; set; }
        public UserState State { get; set; }
        public long TimeStamp { get; set; }
    }
}
