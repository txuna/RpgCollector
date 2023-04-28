using RpgCollector.CustomConstraints;
using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.LoginModel
{
    public class LoginRequest
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
