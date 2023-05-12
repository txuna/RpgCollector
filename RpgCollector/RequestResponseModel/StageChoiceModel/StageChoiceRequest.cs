using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.StageChoiceModel
{
    public class StageChoiceRequest
    {
        [Required]
        public int StageId { get; set; }
    }
}
