using RpgCollector.Models.NoticeModel;

namespace RpgCollector.RequestResponseModel.NoticeGetModel
{
    public class NoticeGetResponse
    {
        public ErrorState Error { get; set; }
        public Notice[] NoticeList { get; set; }
    }
}
