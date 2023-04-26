using Microsoft.AspNetCore.Mvc;
using RpgCollector.Services;

namespace RpgCollector.Controllers
{
    public class LogoutController : Controller
    {
        ICustomAuthenticationService _authenticationService;
        public LogoutController(ICustomAuthenticationService authenticationService) 
        {
            _authenticationService = authenticationService;
        }
        [Route("/Logout")]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var userName = HttpContext.Request.Headers["User-Name"];
            var (success, content) = await _authenticationService.Logout(userName);
            if (!success)
            {
                return BadRequest(content);
            }
            return Ok(content);
        }
    }
}
