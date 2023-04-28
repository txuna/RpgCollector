namespace RpgCollector.RequestResponseModel.MailReadModel
{
    public class MailReadResponse
    {
        public ErrorState Error { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
