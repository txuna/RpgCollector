using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.EnchantReqRes
{
    public class EnchantInfoGetRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string ClientVersion { get; set; }
        [Required]
        public string MasterVersion { get; set; }
        [Required]
        public string AuthToken { get; set; }
        [Required]
        public int PlayerItemId { get; set; }
    }
}
