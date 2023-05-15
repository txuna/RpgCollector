

namespace RpgCollector.Models.StageModel
{
    public class RedisStageItem
    {
        public int ItemId { get; set; }
        public int FarmingCount { get; set; }
        public int MaxCount { get; set; }
    }

    public class RedisStageNpc
    {
        public int NpcId { get; set; }
        public int Count { get; set; }
        public int RemaingCount { get; set; }
        public int Exp { get; set; }
    }


    public class RedisPlayerStageInfo
    {
        public int UserId { get; set; }
        public int StageId { get; set; }
        public RedisStageNpc[] Npcs { get; set; }
        public RedisStageItem[] FarmingItems { get; set; }
        public int RewardExp { get; set; }
    }
}
