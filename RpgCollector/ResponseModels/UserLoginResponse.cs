namespace RpgCollector.ResponseModels
{
    public class UserLoginResponse
    {
        public bool Success { get; set; }
        public string UserName { get; set; }
        public string AuthToken { get; set; }
    }
}
