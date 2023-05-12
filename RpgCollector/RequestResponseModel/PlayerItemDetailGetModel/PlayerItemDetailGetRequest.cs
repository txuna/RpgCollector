using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.PlayerItemDetailGetModel
{
    public class PlayerItemDetailGetRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string ClientVersion { get; set; }
        [Required]
        public string MasterVersion { get; set; }
        [Required]
        public string AuthToken { get; set; }
        public int PlayerItemId { get; set; }
    }
}
