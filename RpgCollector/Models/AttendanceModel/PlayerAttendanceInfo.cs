namespace RpgCollector.Models.AttendanceData
{
    public class PlayerAttendanceInfo
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public int SequenceDayCount { get; set; }
    }
}
