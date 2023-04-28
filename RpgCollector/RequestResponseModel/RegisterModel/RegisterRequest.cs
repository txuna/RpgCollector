using RpgCollector.CustomConstraints;
using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.RegisterModel
{
    public class RegisterRequest
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
