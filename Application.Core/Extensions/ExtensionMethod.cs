using System.Linq.Expressions;

namespace Application.Core.Extensions
{
    public static class ExtensionMethod
    {
        public static DateTime ToLocal(this DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc), TimeZoneInfo.Local);
        }
        public static DateTime ToUtc(this DateTime localDateTime)
        {
            return TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(localDateTime, DateTimeKind.Local), TimeZoneInfo.Utc);
        }
        public static Guid ToGuid(this string aString)
        {
            Guid newGuid;

            if (string.IsNullOrWhiteSpace(aString))
            {
                return Guid.Empty;
            }

            if (Guid.TryParse(aString, out newGuid))
            {
                return newGuid;
            }

            return Guid.Empty;
        }
        public static DateTime FirstDayOfWeek(this DateTime dt)
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var diff = dt.DayOfWeek - culture.DateTimeFormat.FirstDayOfWeek;
            if (diff < 0)
                diff += 7;
            return dt.AddDays(-diff).Date;
        }

        public static DateTime LastDayOfWeek(this DateTime dt)
        {
            return dt.FirstDayOfWeek().AddDays(6);
        }

        public static DateTime FirstDayOfMonth(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1);
        }

        public static DateTime LastDayOfMonth(this DateTime dt)
        {
            return dt.FirstDayOfMonth().AddMonths(1).AddDays(-1);
        }

        public static DateTime FirstDayOfNextMonth(this DateTime dt)
        {
            return dt.FirstDayOfMonth().AddMonths(1);
        }
        public static DateTime TrimSecondsAndMilliseconds(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, 0, dt.Kind);
        }
        public static string ToCamelCase(this string input)
        {
            string[] words = input.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
            }
            return string.Concat(words);
        }
        //public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        //{
        //    var invokedExpr = Expression.Invoke(second, first.Parameters.Cast<Expression>());
        //    return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(first.Body, invokedExpr), first.Parameters);
        //}

    }
}
