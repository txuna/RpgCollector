using RpgCollector.Models.EnchantModel;
using RpgCollector.Models.MasterModel;

namespace RpgCollector.RequestResponseModel.PlayerItemDetailGetModel
{
    public class PlayerItemDetailGetResponse
    {
        public ErrorState Error { get; set; }
        // EnchantCount의 가치만큼 해당 값 수정
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int BaseAttack { get; set; }
        public int BaseMagic { get; set; }
        public int BaseDefence { get; set; }
        public int PlusAttack { get; set; }
        public int PlusDefence { get; set; }
        public int PlusMagic { get; set; }
        public int EnchantCount { get; set; }
        public string AttributeName { get; set; }
        public string TypeName { get; set; }
    }
}
