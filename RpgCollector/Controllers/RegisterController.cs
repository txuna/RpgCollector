using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestModels;
using RpgCollector.Services;

namespace RpgCollector.Controllers
{
    public class RegisterController : Controller
    {
        ICustomAuthenticationService _authenticationService;
        public RegisterController(ICustomAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }
        [Route("/Register")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserRequest userRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Register Request");
            }
            var (success, content) = await _authenticationService.Register(userRequest.UserName, userRequest.Password);
            if (!success)
            {
                return BadRequest(content);
            }
            return Ok(content);
        }
    }
}
