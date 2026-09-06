namespace Application.Core.Common
{
    public class TimeSpanHelper
    {
        public static TimeSpan ParseTime(string? timeString)
        {
            if (string.IsNullOrWhiteSpace(timeString))
            {
                throw new ArgumentNullException(nameof(timeString), "Time string cannot be empty.");
            }

            string[] parts = timeString.Split(new[] { ':', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 3)
            {
                throw new ArgumentException("Invalid time format. Use the format 'h.mm tt' (e.g., '12.20 PM').");
            }

            int hours = int.Parse(parts[0]);
            int minutes = int.Parse(parts[1]);
            string amPm = parts[2].Trim().ToUpper();

            if (hours < 1 || hours > 12 || minutes < 0 || minutes >= 60 || (amPm != "AM" && amPm != "PM"))
            {
                throw new ArgumentException("Invalid time format. Use the format 'h.mm tt' (e.g., '12.20 PM').");
            }

            if (amPm == "PM" && hours < 12)
            {
                hours += 12;
            }

            return new TimeSpan(hours, minutes, 0);
        }
        public static string ConvertMinutesToHoursAndMinutes(int totalMinutes)
        {
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;
            string result = $"{hours:00}:{minutes:00}"; // Formats the hours and minutes as two-digit values.
            return result;
        }
    }
}
