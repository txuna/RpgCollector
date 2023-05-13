using RpgCollector.Models.EnchantModel;
using RpgCollector.Models.MasterModel;

namespace RpgCollector.RequestResponseModel.PlayerReqRes
{
    public class PlayerItemDetailGetResponse
    {
        public ErrorCode Error { get; set; }
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
