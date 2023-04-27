using RpgCollector.Models;
using RpgCollector.Models.MailData;

namespace RpgCollector.ResponseModels.MailResponse
{
    public class MailboxResponse
    {
        public bool Success { get; set; }
        public int TotalPageNumber { get; set; }
        public Mailbox[] Mails { get; set; }
    }
}
