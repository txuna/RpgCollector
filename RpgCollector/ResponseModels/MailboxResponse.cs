using RpgCollector.Models;

namespace RpgCollector.ResponseModels
{
    public class MailboxResponse
    {
        public bool Success { get; set; }
        public int TotalPageNumber { get; set; }
        public Mailbox[] Mails { get; set; }
    }
}
