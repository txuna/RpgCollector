using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.MailReadModel
{
    public class MailReadRequest
    {
        [Required]
        public int MailId { get; set; }
    }
}
