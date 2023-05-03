using RpgCollector.Models.MailModel;

namespace RpgCollector.RequestResponseModel.MailOpenModel
{
    public class OpenMail
    {
        public string Title { get; set; }
        public int SenderId { get; set; }
        public string SendDate { get; set; }
        public int MailId { get; set; }
    }
    public class MailOpenResponse
    {
        public ErrorState Error { get; set; }
        //public Mailbox[] Mails { get; set; }
        public OpenMail[] Mails { get; set; }
    }
}
