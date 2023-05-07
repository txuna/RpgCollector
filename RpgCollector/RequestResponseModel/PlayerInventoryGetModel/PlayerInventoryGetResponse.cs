using RpgCollector.Models.PlayerModel;

namespace RpgCollector.RequestResponseModel.PlayerInventoryGetModel
{
    public class PlayerInventoryGetResponse
    {
        public ErrorState Error { get; set; }
        public PlayerItem[]? Items { get; set; }
    }
}
