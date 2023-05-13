using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.PackageItemModel;
using RpgCollector.RequestResponseModel.PaymentReqRes;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.PackageControllers;

[ApiController]
public class PackageShowController : Controller
{
    IMasterDataDB _masterDataDB;
    ILogger<PackageShowController> _logger; 
    public PackageShowController(ILogger<PackageShowController> logger, IMasterDataDB masterDataDB)
    {
        _logger = logger;
        _masterDataDB = masterDataDB;
    }

    [Route("/Package/Show")]
    [HttpPost]
    public async Task<PackageShowResponse> Index(PackageShowRequest packageShowRequest)
    {
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);

        MasterPackagePayment[] masterPackagePayments = _masterDataDB.GetPackagePayment();

        return new PackageShowResponse
        {
            Error = RequestResponseModel.ErrorCode.None,
            PackagePayment = masterPackagePayments,
        };
    }
}
