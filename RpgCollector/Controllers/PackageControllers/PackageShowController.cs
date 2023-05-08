using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.PackageItemModel;
using RpgCollector.RequestResponseModel.PackageShowModel;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.PackageControllers
{
    [ApiController]
    public class PackageShowController : Controller
    {
        IMasterDataDB _masterDataDB;
        IAccountMemoryDB _accountMemoryDB;
        ILogger<PackageShowController> _logger; 
        public PackageShowController(IAccountMemoryDB accountMemoryDB, ILogger<PackageShowController> logger, IMasterDataDB masterDataDB)
        {
            _accountMemoryDB = accountMemoryDB;
            _logger = logger;
            _masterDataDB = masterDataDB;
        }

        [Route("/Package/Show")]
        [HttpPost]
        public async Task<PackageShowResponse> Index(PackageShowRequest packageShowRequest)
        {
            string userName = HttpContext.Request.Headers["User-Name"];
            int userId = await _accountMemoryDB.GetUserId(userName);

            _logger.ZLogInformation($"[{userId}] Request /Package/Show ");

            if(userId == -1)
            {
                return new PackageShowResponse
                {
                    Error = RequestResponseModel.ErrorState.NoneExistName
                };
            }

            MasterPackagePayment[] masterPackagePayments = _masterDataDB.GetPackagePayment();

            return new PackageShowResponse
            {
                Error = RequestResponseModel.ErrorState.None,
                PackagePayment = masterPackagePayments,
            };
        }
    }
}
