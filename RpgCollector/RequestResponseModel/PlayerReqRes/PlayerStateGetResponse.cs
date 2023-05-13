using RpgCollector.Models.MasterModel;
using RpgCollector.Models.PlayerModel;

namespace RpgCollector.RequestResponseModel.PlayerReqRes
{
    public class PlayerStateGetResponse
    {
        public ErrorCode Error { get; set; }
        public PlayerState State { get; set; }
        public MasterPlayerState MasterState { get; set; }
    }
}
