using RpgCollector.Models;
using RpgCollector.Models.AccountModel;

namespace RpgCollector.RequestResponseModel.AccountReqRes
{
    public class LoginResponse
    {
        public ErrorCode Error { get; set; }
        public string UserName { get; set; }
        public string AuthToken { get; set; }
        public UserState State { get; set; }
    }
}
