namespace RpgCollector.Models
{
    public class User
    {
        public int Index { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public int Permission { get; set; }
    }
}
