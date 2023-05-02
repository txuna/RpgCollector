using RpgCollector.CustomConstraints;
using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.RegisterModel
{
    public class RegisterRequest
    {
        [Required]
        [RegisterUserName]
        public string UserName { get; set; }

        [Required]
        [RegisterUserPassword]
        public string Password { get; set; }
    }
}
