using RpgCollector.Models.MailModel;

namespace RpgCollector.RequestResponseModel.MailReqRes
{
    public class OpenMail
    {
        public string Title { get; set; }
        public int MailId { get; set; }
    }
    public class MailOpenResponse
    {
        public ErrorCode Error { get; set; }
        public int TotalPageNumber { get; set; }
        public OpenMail[] Mails { get; set; }
    }
}
