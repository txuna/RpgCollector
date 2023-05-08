﻿using RpgCollector.Models.MasterModel;
using RpgCollector.Models.PlayerModel;

namespace RpgCollector.RequestResponseModel.PlayerStateGetModel
{
    public class PlayerStateGetResponse
    {
        public ErrorState Error { get; set; }
        public PlayerState State { get; set; }
        public MasterPlayerState MasterState { get; set; }
    }
}