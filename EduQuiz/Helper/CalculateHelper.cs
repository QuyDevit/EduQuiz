namespace EduQuiz.Helper
{
    public class CalculateHelper
    {
        public static double CalculateValueFromPercentage(int percent)
        {
            double minValue = 377.1592641023422;

            // Tính giá trị tương ứng với phần trăm
            double value = minValue - (3.70588235294 * percent);

            return value; 
        }
        public static double CalculateValueFromPercentageQuestion(int percent)
        {
            double minValue = 75.39822368615503;

            // Tính giá trị tương ứng với phần trăm
            double value = minValue - (0.75757575757 * percent);

            return value; 
        }
        public static double CalculateValueFromPercentagePlayer(int percent)
        {
            double minValue = 219.9114857512855;

            // Tính giá trị tương ứng với phần trăm
            double value = minValue - (2.22132818333 * percent);

            return value;
        }
        public static string ConvertDeadline(DateTime deadline)
        {
            TimeSpan timeRemaining = deadline - DateTime.Now;

            if (timeRemaining.TotalSeconds <= 0)
            {
                return "Đã hết hạn";
            }

            if (timeRemaining.Days > 0)
            {
                return $"{timeRemaining.Days} ngày";
            }
            else if (timeRemaining.Hours > 0)
            {
                return $"{timeRemaining.Hours} giờ";
            }
            else
            {
                return $"{timeRemaining.Minutes} phút";
            }
        }
    }
}
