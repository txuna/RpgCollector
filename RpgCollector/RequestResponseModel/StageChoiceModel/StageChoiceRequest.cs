using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.StageChoiceModel
{
    public class StageChoiceRequest
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
        public int StageId { get; set; }
    }
}
