using RpgCollector.Models;

namespace RpgCollector.RequestResponseModel.NoticeGetModel
{
    public class NoticeGetResponse
    {
        public ErrorState Error { get; set; }
        public Notice[] NoticeList { get; set; }
    }
}
