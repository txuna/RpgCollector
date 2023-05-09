using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AttendanceData;
using RpgCollector.RequestResponseModel.AttendanceGetLogModel;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.AttendanceControllers
{
    [ApiController]
    public class AttendanceGetLogController : Controller
    {
        ILogger<AttendanceGetLogController> _logger;
        IAttendanceDB _attendanceDB; 
        public AttendanceGetLogController(IAttendanceDB attendanceDB, 
                                          ILogger<AttendanceGetLogController> logger)
        {
            _attendanceDB = attendanceDB;
            _logger = logger;
        }

        [Route("/Attendance/Log")]
        [HttpPost]
        public async Task<AttendanceGetLogResponse> GetAttendanceLog()
        {
            int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);
            int count;

            _logger.ZLogInformation($"[{userId}] Request /Attendance/Log");

            PlayerAttendanceLog? playerAttendanceLog = await _attendanceDB.GetLastSequenceDayCount(userId);

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
