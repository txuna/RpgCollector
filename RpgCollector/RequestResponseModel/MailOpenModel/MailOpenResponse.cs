using RpgCollector.Models.MailData;

namespace RpgCollector.RequestResponseModel.MailOpenModel
{
    public class MailOpenResponse
    {
        public ErrorState Error { get; set; }
        public Mailbox[] Mails { get; set; }
    }
}
