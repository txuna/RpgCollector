using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestModels;
using RpgCollector.Services;

namespace RpgCollector.Controllers
{
    public class AuthenticationController : Controller
    {
        private ICustomAuthenticationService _authenticationService;
        public AuthenticationController(ICustomAuthenticationService authenticationService) 
        {
            _authenticationService = authenticationService;
        }
        /*
         * 로그인 요청 컨트롤러
         * 1. 모델 바인딩시 유효성 판단 
         * 2. 디비 서비스에 로그인 로직 요청
         * 3. 성공적이라면 content에 마스터 데이터 + 유저 데이터 전송 - redirect로
         */
        [Route("/Login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody]UserRequest userRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Login Request");
            }
            var (success, content) = await _authenticationService.Login(userRequest.UserName, userRequest.Password);
            if(!success)
            {
                return BadRequest(content);
            }
            return Content(content, "application/json");
        }

        
    }
}
