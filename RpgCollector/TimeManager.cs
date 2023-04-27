namespace RpgCollector
{
    public static class TimeManager
    {
        public static DateTime GetDateTime(this long timestamp)
        {
            DateTime currentTime = new DateTime(); // UTC 기준의 1970년 1월 1일 0시 0분 0초를 나타내는 DateTime 객체 생성
            currentTime = currentTime.AddMilliseconds(timestamp); // Timestamp 값을 더하여 현재 시간을 구함

            return currentTime;
        }

        public static bool IsTimeOut(this long timestamp)
        {
            long waitTimeout = 3600000;
            long nowTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            // 활동 기록이 waitTimeout보다 길다면 세션활동이 없는것 
            if (nowTimestamp - timestamp > waitTimeout)
            {
                return true;
            }

            return false;
        }

        public static long GetTimeStamp()
        {
            long nowTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            return nowTimestamp;
        }
    }
}
