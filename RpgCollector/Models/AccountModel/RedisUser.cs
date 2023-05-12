namespace RpgCollector.Models.AccountModel
{
    public enum UserState
    {
        Login = 1, 
        Playing = 2,
    }
    public class RedisUser
    {
        public int UserId { get; set; }
        public string AuthToken { get; set; }
        public UserState State { get; set; }
    }
}
