using RpgCollector.CustomConstraints;
using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestModels
{
    public class UserRequest
    {
        [Required]
        [CustomUserName]
        public string UserName { get; set; }

        [Required]
        [CustomUserPassword]
        public string Password { get; set; }
    }

}
