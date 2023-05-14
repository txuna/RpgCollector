using RpgCollector.Models.StageModel;

namespace RpgCollector.RequestResponseModel.DungeonStageReqRes
{
    public class StagePlayingInfoLoadResponse
    {
        public ErrorCode Error { get; set; }
        public RedisStageItem[] Items { get; set; }
        public RedisStageNpc[] Npcs { get; set; }
        public int StageId { get; set; }
    }
}
