using RpgCollector.Models.MailModel;

namespace RpgCollector.RequestResponseModel.MailReadModel
{
    public class MailReadResponse
    {
        public ErrorState Error { get; set; }
        public Mailbox Mail { get; set; }
        public MailItem? MailItem { get; set; }
    }
}
