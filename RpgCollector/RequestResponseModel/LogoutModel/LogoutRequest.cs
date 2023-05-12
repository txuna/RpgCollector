using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.LogoutModel
{
    public class LogoutRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string ClientVersion { get; set; }
        [Required]
        public string MasterVersion { get; set; }
        [Required]
        public string AuthToken { get; set; }
    }
}
