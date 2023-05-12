namespace RpgCollector.RequestResponseModel.StageInfoGetModel
{
    public class Stage
    {
        public int StageId { get; set; }
        public bool IsOpen { get; set; }
    }
    public class StageInfoGetResponse
    {
        public ErrorCode Error { get; set; }
        public Stage[]? Stages { get; set; }
    }
}
