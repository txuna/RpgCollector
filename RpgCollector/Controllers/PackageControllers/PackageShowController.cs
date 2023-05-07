using Microsoft.AspNetCore.Mvc;
using RpgCollector.Services;

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
        public IActionResult Index()
        {
            return View();
        }
    }
}
