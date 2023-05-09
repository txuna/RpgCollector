using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AttendanceData;
using RpgCollector.RequestResponseModel.MasterAttendanceInfoModel;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.MasterDataControllers
{
    [ApiController]
    public class MasterAttendanceInfoController : Controller
    {
        IMasterDataDB _masterDataDB;
        ILogger<MasterAttendanceInfoController> _logger;
        IAccountMemoryDB _accountMemoryDB;
        public MasterAttendanceInfoController(IMasterDataDB masterDataDB, ILogger<MasterAttendanceInfoController> logger, IAccountMemoryDB accountMemoryDB)
        {
            _masterDataDB = masterDataDB;
            _logger = logger;
            _accountMemoryDB = accountMemoryDB;
        }

        [Route("/Master/Attendance")]
        [HttpPost]
        public async Task<MasterAttendanceInfoResponse> GetAttendanceInfo(MasterAttendanceInfoRequest masterAttendanceInfoRequest)
        {
            string userName = HttpContext.Request.Headers["User-Name"];
            int userId = await _accountMemoryDB.GetUserId(userName);

            _logger.ZLogInformation($"[{userId}] Request /Master/Attendance");

            if (userId == -1)
            {
                return new MasterAttendanceInfoResponse
                {
                    Error = RequestResponseModel.ErrorState.NoneExistName
                };
            }

            MasterAttendanceReward[] masterAttendanceRewards = _masterDataDB.GetAllMasterAttendanceReward();

            return new MasterAttendanceInfoResponse
            {
                Error = RequestResponseModel.ErrorState.None,
                AttendanceRewards = masterAttendanceRewards,
            };
        }
    }
}
