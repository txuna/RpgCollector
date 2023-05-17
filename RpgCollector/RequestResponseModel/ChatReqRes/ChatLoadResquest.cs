using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.ChatReqRes
{
    public class ChatLoadResquest
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
        public Int64 TimeStamp { get; set; }
    }
}
