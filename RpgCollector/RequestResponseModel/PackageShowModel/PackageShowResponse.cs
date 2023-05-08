using RpgCollector.Models.PackageItemModel;

namespace RpgCollector.RequestResponseModel.PackageShowModel
{
    public class PackageShowResponse
    {
        public ErrorState Error { get; set; }
        public MasterPackagePayment[] PackagePayment { get; set; }
    }
}
