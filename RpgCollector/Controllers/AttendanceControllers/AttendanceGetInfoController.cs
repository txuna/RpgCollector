using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AttendanceData;
using RpgCollector.RequestResponseModel.AttendanceReqRes;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.AttendanceControllers;

[ApiController]
public class AttendanceGetInfoController : Controller
{
    ILogger<AttendanceGetInfoController> _logger;
    IAttendanceDB _attendanceDB; 
    public AttendanceGetInfoController(IAttendanceDB attendanceDB, 
                                      ILogger<AttendanceGetInfoController> logger)
    {
        _attendanceDB = attendanceDB;
        _logger = logger;
    }

    [Route("/Attendance/Log")]
    [HttpPost]
    public async Task<AttendanceGetLogResponse> GetAttendanceLog()
    {
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);

        _logger.ZLogDebug($"[{userId}] Request /Attendance/Log");

        int count = await _attendanceDB.GetUserSequenceDayCount(userId);


        return new AttendanceGetLogResponse
        {
            Error = RequestResponseModel.ErrorCode.None,
            SequenceDayCount = count,
        };
    }
}
