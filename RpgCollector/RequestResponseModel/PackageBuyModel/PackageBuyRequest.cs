using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.PaymentModel
{
    public class PackageBuyRequest
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
        public int ReceiptId { get; set; }
        [Required]
        public int PackageId { get; set; }
    }
}
