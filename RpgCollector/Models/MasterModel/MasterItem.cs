namespace RpgCollector.Models.MasterModel
{
    public class MasterItem
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int AttributeId { get; set; }
        public int SellPrice { get; set; }
        public int BuyPrice { get; set; }
        public int CanLevel { get; set; }
        public int Attack { get; set; }
        public int Defence { get; set; }
        public int Magic { get; set; }
        public int MaxEnchantCount { get; set; }
    }
}
