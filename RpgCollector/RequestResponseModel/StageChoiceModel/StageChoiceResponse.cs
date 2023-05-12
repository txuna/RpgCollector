using RpgCollector.Models.MasterModel;

namespace RpgCollector.RequestResponseModel.StageChoiceModel
{
    public class StageChoiceResponse
    {
        public ErrorCode Error { get; set; }
        public MasterStageItem[] masterStageItem { get; set; }
        public MasterStageNpc[] masterStageNpc { get; set;}
    }
}
