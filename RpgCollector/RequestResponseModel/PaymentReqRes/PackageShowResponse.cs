using RpgCollector.Models.PackageItemModel;

namespace RpgCollector.RequestResponseModel.PaymentReqRes
{
    public class PackageShowResponse
    {
        public ErrorCode Error { get; set; }
        public MasterPackagePayment[] PackagePayment { get; set; }
    }
}
