using RpgCollector.CustomConstraints;
using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.AccountReqRes
{
    public class RegisterRequest
    {
        [Required]
        [RegisterUserName]
        public string UserName { get; set; }

        [Required]
        [RegisterUserPassword]
        public string Password { get; set; }
        [Required]
        public string ClientVersion { get; set; }
        [Required]
        public string MasterVersion { get; set; }
        [Required]
        public string AuthToken { get; set; }
    }
}
