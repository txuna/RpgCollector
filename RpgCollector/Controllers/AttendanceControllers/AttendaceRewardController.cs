using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel.AttandenceModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.Services;
using RpgCollector.Models.AttendanceData;
using Microsoft.Extensions.Logging;
using ZLogger;
using RpgCollector.Models.AccountModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RpgCollector.Controllers.AttandanceControllers;

[ApiController]
public class AttendaceRewardController : Controller
{
    IMailboxAccessDB _mailboxAccessDB;
    IAttendanceDB _attendanceDB;
    IMasterDataDB _masterDataDB;
    readonly ILogger<AttendaceRewardController> _logger;

    public AttendaceRewardController(IMailboxAccessDB mailboxAccessDB, 
                                     IAttendanceDB attendanceDB, 
                                     IMasterDataDB masterDataDB,
                                     ILogger<AttendaceRewardController> logger)
    {
        _mailboxAccessDB = mailboxAccessDB;
        _attendanceDB = attendanceDB;
        _masterDataDB = masterDataDB;
        _logger = logger;
    }

    /*
    * 서버시간을 기준으로 사용자의 출석을 진행하는 API 
    * - 연속 출석일수에 맞춰 보상을 지급한다. 
    * - 보상은 아이템이 동봉된 메일을 전송한다.
    */
    [Route("/Attendance")]
    [HttpPost]
    public async Task<AttendanceResponse> Attendance()
    {
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);

        string toDay = DateTime.Now.ToString("yyyy-MM-dd");
        string yesterDay = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

        ErrorState Error = await IsTodayAttendance(userId, toDay);

        if(Error != ErrorState.None)
        {
            _logger.ZLogInformation($"[{userId}] Today Already Attandace UserID");

            return new AttendanceResponse
            {
                Error = Error
            };
        }

        Error = await DoAttendance(userId, yesterDay, toDay);

        if(Error != ErrorState.None)
        {
            _logger.ZLogError($"[{userId}] Failed Attandace");

            return new AttendanceResponse
            {
                Error = Error
            };
        }

        _logger.ZLogInformation($"[{userId}] Complement Send Attandace Reward");

        return new AttendanceResponse
        {
            Error = ErrorState.None
        };
    }

    async Task<ErrorState> IsTodayAttendance(int userId, string toDay)
    {
        if(await _attendanceDB.IsAttendance(userId, toDay))
        {
            return ErrorState.AlreadyAttendance;
        }

        return ErrorState.None;
    }

    async Task<int> GetLastSequenceDayCount(int userId, string yesterDay)
    {
        if(!await IsYesterdayAttendance(userId, yesterDay))
        {
            return 1;
        }

        int sequenceDayCount = await _attendanceDB.GetUserSequenceDayCount(userId);

        return (sequenceDayCount + 1) % 31;
    }

    async Task<bool> IsYesterdayAttendance(int userId, string yesterDay)
    {
        if(await _attendanceDB.IsAttendance(userId, yesterDay))
        {
            return true;
        }

        return false;
    }

    async Task<ErrorState> DoAttendance(int userId, string yesterDay, string toDay)
    {
        int sequenceDayCount = await GetLastSequenceDayCount(userId, yesterDay);

        if (!await _attendanceDB.DoAttendance(userId, sequenceDayCount))
        {
            return ErrorState.FailedAttendance;
        }

        ErrorState Error = await SendAttendanceReward(userId, sequenceDayCount);
        
        if(Error != ErrorState.None)
        {
            Error = await UndoAttendance(userId, toDay);
            return Error;
        }

        return ErrorState.None;
    }

    async Task<ErrorState> SendAttendanceReward(int userId, int sequenceDayCount)
    {
        MasterAttendanceReward? reward = _masterDataDB.GetMasterAttendanceReward(sequenceDayCount);
        if (reward == null)
        {
            return ErrorState.FailedSendAttendanceReward;
        }

        await _mailboxAccessDB.SendMail(1, userId, "Attendance Reward", $"Thanks for your Sequence Attendance! {sequenceDayCount} Days!", reward.ItemId, reward.Quantity);
        
        return ErrorState.None;
    }
    
    async Task<ErrorState> UndoAttendance(int userId, string toDay)
    {
        if(!await _attendanceDB.UndoAttendance(userId, toDay))
        {
            return ErrorState.FailedUndoAttendance;
        }

        return ErrorState.None;
    }
}
