using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestModels.MailRequest
{
    public class OpenMailboxRequest
    {
        [Required]
        public bool IsFirstOpen { get; set; }
        [Required]
        public int PageNumber { get; set; }
    }
}
