using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.PaymentModel
{
    public class PackageBuyRequest
    {
        [Required]
        public int ReceiptId { get; set; }
        [Required]
        public int PackageId { get; set; }
    }
}
