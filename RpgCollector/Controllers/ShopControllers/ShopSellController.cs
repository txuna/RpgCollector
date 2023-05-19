using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel.ShopReqRes;

namespace RpgCollector.Controllers.ShopControllers
{
    [ApiController]
    public class ShopSellController : Controller
    {
        ILogger<ShopSellController> _logger;
        public ShopSellController(ILogger<ShopSellController> logger)
        {
            _logger = logger;
        }


        [Route("/Shop/Sell")]
        [HttpPost]
        public async Task<ShopSellProductResponse> Post(ShopSellProductRequest shopSellProductRequest)
        {
            // load player item 

            // load master item 

            // sell item 

            return new ShopSellProductResponse
            {
                Error = RequestResponseModel.ErrorCode.None
            };
        }
    }
}
