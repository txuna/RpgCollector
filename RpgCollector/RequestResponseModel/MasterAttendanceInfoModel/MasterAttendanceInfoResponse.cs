using RpgCollector.Models.AttendanceData;

namespace RpgCollector.RequestResponseModel.MasterAttendanceInfoModel
{
    public class MasterAttendanceInfoResponse
    {
        public ErrorCode Error { get; set; }
        public MasterAttendanceReward[] AttendanceRewards { get; set; }
    }
}
