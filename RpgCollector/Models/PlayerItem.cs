namespace RpgCollector.Models
{
    public class PlayerItem
    {
        public int PlayerItemId { get; set; }
        public int UserId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public int EnchantCount { get; set; }
    }
}
