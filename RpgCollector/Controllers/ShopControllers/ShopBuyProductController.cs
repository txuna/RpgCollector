using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RpgCollector.RequestResponseModel.ShopReqRes;
using RpgCollector.Services;

namespace RpgCollector.Controllers.ShopControllers;

[ApiController]
public class ShopBuyProductController : Controller
{
    IMasterDataDB _masterDataDB;
    ILogger<ShopBuyProductController> _logger;
    public ShopBuyProductController(IMasterDataDB masterDataDB, ILogger<ShopBuyProductController> logger)
    {
        _logger = logger;
        _masterDataDB = masterDataDB;
    }

    [Route("/Shop/Buy")]
    [HttpPost]
    public async Task<ShopBuyProductResponse> Post(ShopBuyProductRequest shopBuyProductRequest)
    {
        return new ShopBuyProductResponse
        {
            Error = RequestResponseModel.ErrorCode.None
        };
    }
}
