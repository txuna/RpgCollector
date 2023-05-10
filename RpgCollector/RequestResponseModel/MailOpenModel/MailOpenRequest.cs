using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.MailOpenModel
{
    public class MailOpenRequest
    {
        [Required]
        public int? PageNumber { get; set; }
    }
}
