﻿using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.DungeonStageReqRes
{
    public class StageClearRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string ClientVersion { get; set; }
        [Required]
        public string MasterVersion { get; set; }
        [Required]
        public string AuthToken { get; set; }

        [Required]
        public int PlayerHp { get; set; }
    }
}
