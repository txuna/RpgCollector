namespace RpgCollector.Models
{
    public class MailItem
    {
        public int MailId { get; set; }
        public int itemId { get; set; }
        public int Quantity { get; set; }
        public int HasReceived { get; set; }
    }
}
