using RpgCollector.Models;

namespace RpgCollector.ResponseModels
{
    public class NoticeResponse
    {
        public bool Success { get; set; }
        public Notice[] NoticeList { get; set; }
    }
}
