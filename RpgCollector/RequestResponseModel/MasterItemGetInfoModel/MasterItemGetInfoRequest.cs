using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.MasterItemGetInfoModel
{
    public class MasterItemGetInfoRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string ClientVersion { get; set; }
        [Required]
        public string MasterVersion { get; set; }
        [Required]
        public string AuthToken { get; set; }
        public int ItemId { get; set; }
    }
}
