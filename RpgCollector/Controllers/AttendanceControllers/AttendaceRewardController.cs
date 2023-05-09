using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel.AttandenceModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.Services;
using RpgCollector.Models.AttendanceData;
using Microsoft.Extensions.Logging;
using ZLogger;
using RpgCollector.Models.AccountModel;

namespace RpgCollector.Controllers.AttandanceControllers;

[ApiController]
public class AttendaceRewardController : Controller
{
    IMailboxAccessDB _mailboxAccessDB;
    IAccountMemoryDB _accountMemoryDB;
    IAttendanceDB _attendanceDB;
    IMasterDataDB _masterDataDB;
    readonly ILogger<AttendaceRewardController> _logger;

    public AttendaceRewardController(IMailboxAccessDB mailboxAccessDB, 
                                     IAttendanceDB attendanceDB, 
                                     IMasterDataDB masterDataDB,
                                     IAccountMemoryDB accountMemoryDB,
                                     ILogger<AttendaceRewardController> logger)
    {
        _mailboxAccessDB = mailboxAccessDB;
        _attendanceDB = attendanceDB;
        _masterDataDB = masterDataDB;
        _accountMemoryDB = accountMemoryDB;
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
        string userName = HttpContext.Request.Headers["User-Name"];
        int userId = await _accountMemoryDB.GetUserId(userName);

        string toDay = DateTime.Now.ToString("yyyy-MM-dd");
        string yesterDay = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

        ErrorState Error;

        _logger.ZLogInformation($"[{userId} {userName}] Request 'Attendance'");

        Error = await IsTodayAttendance(userId, toDay);

        if(Error != ErrorState.None)
        {
            _logger.ZLogInformation($"[{userName} : {userId}] Today Already Attandace UserID");
            return new AttendanceResponse
            {
                Error = Error
            };
        }

        int sequenceDayCount = await GetLastSequenceDayCount(userId, yesterDay);

        Error = await DoAttendance(userId, sequenceDayCount);

        if(Error != ErrorState.None)
        {
            _logger.ZLogError($"[{userName} : {userId}] Failed Attandace");
            return new AttendanceResponse
            {
                Error = Error
            };
        }

        Error = await SendAttendanceReward(userId, sequenceDayCount);

        if(Error  != ErrorState.None)
        {
            _logger.ZLogError($"[{userName} : {userId}] Failed Send Attandace Reward");
            Error = await UndoAttendance(userId, toDay);

            return new AttendanceResponse
            {
                Error = Error
            };
        }

        _logger.ZLogInformation($"[{userName} : {userId}] Complement Send Attandace Reward");

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

    // 해당 유저의 가장 최근의 출석로그를 기반으로 판단 
    async Task<int> GetLastSequenceDayCount(int userId, string yesterDay)
    {
        if(!await IsYesterdayAttendance(userId, yesterDay))
        {
            return 1;
        }

        PlayerAttendanceLog? lastLog = await _attendanceDB.GetLastAttendanceLog(userId);

        if(lastLog == null)
        {
            return 1;
        }

        return (lastLog.SequenceDayCount + 1) % 31;
    }

    async Task<bool> IsYesterdayAttendance(int userId, string yesterDay)
    {
        if(await _attendanceDB.IsAttendance(userId, yesterDay))
        {
            return true;
        }

        return false;
    }

    async Task<ErrorState> DoAttendance(int userId, int sequenceDayCount)
    {
        if(!await _attendanceDB.DoAttendance(userId, sequenceDayCount))
        {
            return ErrorState.FailedAttendance;
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
