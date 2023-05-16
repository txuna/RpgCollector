using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel.ShopReqRes;
using RpgCollector.Services;

namespace RpgCollector.Controllers.ShopControllers;

[ApiController]
public class ShopLoadProductController : Controller
{
    IMasterDataDB _masterDataDB;
    ILogger<ShopLoadProductController> _logger;
    public ShopLoadProductController(IMasterDataDB masterDataDB, ILogger<ShopLoadProductController> logger)
    {
        _logger = logger;
        _masterDataDB = masterDataDB;
    }

    [Route("/Shop/Products")]
    [HttpPost]
    public ShopLoadProductResponse Post(ShopLoadProductRequest shopLoadProductRequest)
    {
        return new ShopLoadProductResponse
        {
            Error = RequestResponseModel.ErrorCode.None
        };
    }
}
