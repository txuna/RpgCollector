using RpgCollector.Models.MasterModel;

namespace RpgCollector.RequestResponseModel.MasterItemGetInfoModel
{
    public class MasterItemGetInfoResponse
    {
        public ErrorCode Error { get; set; }
        public MasterItem MasterItem { get; set; }
        public string AttributeName { get; set; }
        public string TypeName { get; set; }
    }
}
