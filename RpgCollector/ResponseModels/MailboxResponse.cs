using RpgCollector.Models;

namespace RpgCollector.ResponseModels
{
    public class MailboxResponse
    {
        public int TotlaPageNumber { get; set; }
        public Mailbox[] Mails;
    }
}
