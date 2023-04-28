using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.MailOpenModel
{
    public class MailOpenRequest
    {
        [Required]
        public bool? IsFirstOpen { get; set; }

        [Required]
        public int? PageNumber { get; set; }
    }
}
