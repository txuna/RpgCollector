using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AttendanceData;
using RpgCollector.RequestResponseModel.MasterAttendanceInfoModel;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.MasterDataControllers;

[ApiController]
public class MasterAttendanceInfoController : Controller
{
    IMasterDataDB _masterDataDB;
    ILogger<MasterAttendanceInfoController> _logger;
    public MasterAttendanceInfoController(IMasterDataDB masterDataDB, ILogger<MasterAttendanceInfoController> logger)
    {
        _masterDataDB = masterDataDB;
        _logger = logger;
    }

    [Route("/Master/Attendance")]
    [HttpPost]
    public MasterAttendanceInfoResponse GetAttendanceInfo(MasterAttendanceInfoRequest masterAttendanceInfoRequest)
    {
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);

        MasterAttendanceReward[] masterAttendanceRewards = _masterDataDB.GetAllMasterAttendanceReward();

        return new MasterAttendanceInfoResponse
        {
            Error = RequestResponseModel.ErrorCode.None,
            AttendanceRewards = masterAttendanceRewards,
        };
    }
}
