namespace RpgCollector.Models.MailModel
{
    public class MailItem
    {
        public int MailId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public int HasReceived { get; set; }
    }
}
