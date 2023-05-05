using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AttendanceData;
using RpgCollector.RequestResponseModel.AttendanceGetLogModel;
using RpgCollector.Services;

namespace RpgCollector.Controllers.AttendanceControllers
{
    [ApiController]
    public class AttendanceGetLogController : Controller
    {
        ILogger<AttendanceGetLogController> _logger;
        IAccountMemoryDB _accountMemoryDB;
        IAttendanceDB _attendanceDB; 
        public AttendanceGetLogController(IAttendanceDB attendanceDB, ILogger<AttendanceGetLogController> logger, IAccountMemoryDB accountMemoryDB)
        {
            _attendanceDB = attendanceDB;
            _logger = logger;
            _accountMemoryDB = accountMemoryDB;
        }
        [Route("/Attendance/Log")]
        [HttpPost]
        public async Task<AttendanceGetLogResponse> GetAttendanceLog()
        {
            string userName = HttpContext.Request.Headers["User-Name"];
            int userId = await _accountMemoryDB.GetUserId(userName);
            PlayerAttendanceLog? playerAttendanceLog = await _attendanceDB.GetLastSequenceDayCount(userId);
            int count;

            if (playerAttendanceLog == null)
            {
                count = 0;
            }
            else
            {
                count = playerAttendanceLog.SequenceDayCount;
            }

            return new AttendanceGetLogResponse
            {
                Error = RequestResponseModel.ErrorState.None,
                SequenceDayCount = count,
            };
        }
    }
}
