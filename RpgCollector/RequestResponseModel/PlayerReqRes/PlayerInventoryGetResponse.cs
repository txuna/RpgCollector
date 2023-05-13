using RpgCollector.Models.PlayerModel;

namespace RpgCollector.RequestResponseModel.PlayerReqRes
{
    public class PlayerInventoryGetResponse
    {
        public ErrorCode Error { get; set; }
        public PlayerItem[]? Items { get; set; }
    }
}
