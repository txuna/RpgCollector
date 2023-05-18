using RpgCollector.Models.ChatModel;

namespace RpgCollector.RequestResponseModel.ChatReqRes
{
    public class ChatLoadResponse
    {
        public ErrorCode Error { get; set; }
        public Chat[] ChatLog { get; set; }
        public Int64 TimeStamp { get; set; }
    }
}
