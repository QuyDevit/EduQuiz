using System.Globalization;

namespace EduQuiz.Helper
{
    public class StringHelper
    {
        public static string ConvertDateTimeToCustomString(DateTime dateTime)
        {
            var vietnamCulture = new CultureInfo("vi-VN");
            return dateTime.ToString("'Ngày' dd 'tháng' M 'năm' yyyy, h:mm tt", vietnamCulture);
        }
        public static string ConvertDateTimeToCustomShortString(DateTime dateTime)
        {
            var vietnamCulture = new CultureInfo("vi-VN");
            return dateTime.ToString("'Ngày' dd'-'M'-'yyyy, h:mm tt", vietnamCulture);
        }
        public static double CalculateSessionDurationInMinutes(DateTime startTime, DateTime endTime)
        {
            // Ensure endTime is greater than startTime
            if (endTime < startTime)
            {
                throw new ArgumentException("End time must be greater than start time.");
            }

            return (endTime - startTime).TotalMinutes;
        }
    }
}
