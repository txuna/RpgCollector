﻿namespace RpgCollector.Models.AttendanceData
{
    public class PlayerAttendanceLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string AttendanceDay { get; set; }
        public int SequenceDayCount { get; set; }
    }
}