namespace Application.Core.Common
{
    public class DateTimeHelper
    {
        public static DateTime UtcToLocal(DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc), TimeZoneInfo.Local);
        }
        public static DateTime LocalToUtc(DateTime localDateTime)
        {
            return TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(localDateTime, DateTimeKind.Local), TimeZoneInfo.Utc);
        }

        public static DateTime UtcToLocalByTimeZoneId(DateTime utcDateTime)
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Bangladesh Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
        }
        public static DateTime LocalToUtcByTimeZoneId(DateTime localDateTime)
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Bangladesh Standard Time");
            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone);
        }
    }
}
