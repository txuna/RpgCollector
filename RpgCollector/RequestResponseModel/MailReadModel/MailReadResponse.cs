using RpgCollector.Models.MailModel;

namespace RpgCollector.RequestResponseModel.MailReadModel
{
    public class MailReadResponse
    {
        public ErrorState Error { get; set; }
        public int MailId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string SendDate { get; set; }
        public int HasItem { get; set; }
        public MailItem? MailItem { get; set; }
    }
}
