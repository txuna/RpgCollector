using RpgCollector.Models;

namespace RpgCollector.RequestResponseModel.AccountReqRes
{
    public class LoginResponse
    {
        public ErrorCode Error { get; set; }
        public string UserName { get; set; }
        public string AuthToken { get; set; }
    }
}
