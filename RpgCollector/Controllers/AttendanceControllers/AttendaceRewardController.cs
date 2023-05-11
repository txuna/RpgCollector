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

        int sequenceDayCount = 1;

        PlayerAttendanceInfo? playerAttendanceInfo = await GetUserAttendance(userId);

        if (playerAttendanceInfo != null)
        {
            if (IsAttendanceDay(playerAttendanceInfo, toDay) == true)
            {
                _logger.ZLogInformation($"[{userId}] Today Already Attandace UserID");

                return new AttendanceResponse
                {
                    Error = ErrorCode.AlreadyAttendance
                };
            }

            if (IsAttendanceDay(playerAttendanceInfo, yesterDay) == true)
            {
                sequenceDayCount = playerAttendanceInfo.SequenceDayCount + 1;
            }
        }

        ErrorCode Error = await DoAttendance(userId, sequenceDayCount % 31);

        if(Error != ErrorCode.None)
        {
            _logger.ZLogError($"[{userId}] Failed Attandace");

            Error = await UndoAttendance(userId, toDay);

            return new AttendanceResponse
            {
                Error = Error
            };
        }

        _logger.ZLogInformation($"[{userId}] Complement Send Attandace Reward");

        return new AttendanceResponse
        {
            Error = ErrorCode.None
        };
    }

    bool IsAttendanceDay(PlayerAttendanceInfo info, string day)
    {
        if(info.Date.ToString("yyyy-MM-dd") == day)
        {
            return true; 
        }

        return false;
    }

    async Task<PlayerAttendanceInfo?> GetUserAttendance(int userId)
    {
        PlayerAttendanceInfo? info = await _attendanceDB.GetUserAttendanceInfo(userId);
        return info;
    }

    async Task<ErrorCode> DoAttendance(int userId, int sequenceDayCount)
    {
        sequenceDayCount = sequenceDayCount == 0 ? 1 : sequenceDayCount;

        if (await _attendanceDB.DoAttendance(userId, sequenceDayCount) == false)
        {
            return ErrorCode.FailedAttendance;
        }

        ErrorCode Error = await SendAttendanceReward(userId, sequenceDayCount);
        
        if(Error != ErrorCode.None)
        {
            return ErrorCode.FailedSendAttendanceReward;
        }

        return ErrorCode.None;
    }

    async Task<ErrorCode> SendAttendanceReward(int userId, int sequenceDayCount)
    {
        MasterAttendanceReward? reward = _masterDataDB.GetMasterAttendanceReward(sequenceDayCount);
        if (reward == null)
        {
            return ErrorCode.FailedSendAttendanceReward;
        }

        await _mailboxAccessDB.SendMail(1, userId, "Attendance Reward", $"Thanks for your Sequence Attendance! {sequenceDayCount} Days!", reward.ItemId, reward.Quantity);
        
        return ErrorCode.None;
    }
    
    async Task<ErrorCode> UndoAttendance(int userId, string toDay)
    {
        if(await _attendanceDB.UndoAttendance(userId, toDay) == false)
        {
            return ErrorCode.FailedUndoAttendance;
        }

        return ErrorCode.None;
    }
}
