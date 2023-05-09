using RpgCollector.Models.EnchantModel;
using RpgCollector.Models.MasterModel;

namespace RpgCollector.RequestResponseModel.PlayerItemDetailGetModel
{
    public class PlayerItemDetailGetResponse
    {
        public ErrorState Error { get; set; }
        // EnchantCount의 가치만큼 해당 값 수정
        public MasterItem ItemPrototype { get; set; }
        public AdditionalState PlusState { get; set; }
        public int EnchantCount { get; set; }
        public string AttributeName { get; set; }
        public string TypeName { get; set; }
    }
}
