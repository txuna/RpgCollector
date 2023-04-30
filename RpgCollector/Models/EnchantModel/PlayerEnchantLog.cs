namespace RpgCollector.Models.EnchantModel
{
    public class PlayerEnchantLog
    {
        public int Index { get; set; }
        public int PlayerItemId { get; set; }
        public int UserId { get; set; }
        public int EnchantCount { get; set; }
        public int Result { get; set; }
        public string Date { get; set; }
    }
}
