using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.ChatReqRes
{
    public class ChatSendRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string ClientVersion { get; set; }
        [Required]
        public string MasterVersion { get; set; }
        [Required]
        public string AuthToken { get; set; }
        public string Content { get; set; }
    }
}
