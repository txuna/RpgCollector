using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.MailDeleteModel
{
    public class MailDeleteRequest
    {
        [Required]
        public int MailId { get; set; }
    }
}
