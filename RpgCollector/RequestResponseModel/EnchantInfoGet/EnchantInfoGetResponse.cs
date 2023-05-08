namespace RpgCollector.RequestResponseModel.EnchantInfoGet
{
    public class EnchantInfoGetResponse
    {
        public ErrorState Error { get; set; }
        public int CurrentEnchantCount { get; set; }
        public int NextEnchantCount { get; set; }
        public int Percent { get; set; }
        public int IncreasementValue { get; set; }
        public int ItemId { get; set; }
        public int PlayerItemId { get; set; }
        public int Price { get; set; }
    }
}
