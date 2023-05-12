using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.MailGetItemModel
{
    public class MailGetItemRequest
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
        public int MailId { get; set; }
    }
}
