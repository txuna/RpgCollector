namespace RpgCollector.ResponseModels
{
    public class PlayerDataResponse
    {
        public int UserId { get; set; }
        public int CurrentHealth { get; set; }
        public int MaxHealth { get; set; }
        public int CurrentExp { get; set; }
        public int MaxExp { get; set; }
        public int Level { get; set; }
        public int Money { get; set; }
    }

    public class PlayerInventoryResponse
    {
        public int PlayerId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class PlayerEnchantResponse
    {
        public int PlayerId { get; set; }
        public int ItemId { get; set; }
        public int RankId { get; set; }
    }
}
