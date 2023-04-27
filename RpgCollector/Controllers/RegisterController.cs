using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestModels;
using RpgCollector.Services;

namespace RpgCollector.Controllers
{
    public class RegisterController : Controller
    {
        ICustomAuthenticationService _authenticationService;
        IPlayerService _playerService;

        public RegisterController(ICustomAuthenticationService authenticationService, IPlayerService playerService)
        {
            _authenticationService = authenticationService;
            _playerService = playerService;
        }

        /**
         *  회원가입을 진행하는 API 
         *  성공적으로 회원가입시 Player Data 디비 초기화 진행 
         */
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
            /* 
             * 계정생성이 완료되면 Player 데이터 생성
             */
            int userId = await _authenticationService.GetUserId(userRequest.UserName);
            (success, content) = await _playerService.CreatePlayer(userId);
            if(!success)
            {
                return BadRequest(content);
            }
            return Ok(content);
        }
    }
}
