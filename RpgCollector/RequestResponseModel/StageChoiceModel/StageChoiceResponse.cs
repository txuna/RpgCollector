using RpgCollector.Models.MasterModel;

namespace RpgCollector.RequestResponseModel.StageChoiceModel
{
    public class StageItem
    {
        public int ItemId { get; set; }
    }

    public class StageNpc
    {
        public int NpcId { get; set; }
        public int Count { get; set; }
    }

    public class StageChoiceResponse
    {
        public ErrorCode Error { get; set; }
        public StageItem[] Items { get; set; }
        public StageNpc[] Npcs { get; set; }
    }
}
