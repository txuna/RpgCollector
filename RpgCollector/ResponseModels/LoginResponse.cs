namespace RpgCollector.ResponseModels
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string UserName { get; set; }
        public string AuthToken { get; set; }
    }
}
