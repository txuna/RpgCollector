using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.DungeonStageReqRes
{
    public class StagePlayingInfoLoadRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string ClientVersion { get; set; }
        [Required]
        public string MasterVersion { get; set; }
        [Required]
        public string AuthToken { get; set; }
    }
}
