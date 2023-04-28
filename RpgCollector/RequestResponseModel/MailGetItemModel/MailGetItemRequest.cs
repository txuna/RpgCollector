using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.MailGetItemModel
{
    public class MailGetItemRequest
    {
        [Required]
        public int MailId { get; set; }
    }
}
