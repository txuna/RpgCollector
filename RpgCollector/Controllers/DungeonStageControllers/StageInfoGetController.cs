using Microsoft.AspNetCore.Mvc;

namespace RpgCollector.Controllers.DungeonStageControllers
{
    [ApiController]
    public class StageInfoGetController : Controller
    {
        [Route("/Stage/Info")]
        [HttpPost]
        public IActionResult StageInfoGet()
        {
            return View();
        }
    }
}
