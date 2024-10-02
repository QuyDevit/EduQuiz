namespace EduQuiz.Helper
{
    public class CalculateHelper
    {
        public static double CalculateValueFromPercentage(int percent)
        {
            double minValue = 377.1592641023422;

            // Tính giá trị tương ứng với phần trăm
            double value = minValue - (3.70588235294 * percent);

            return value; // Làm tròn đến 3 chữ số sau dấu phẩy
        }
        public static double CalculateValueFromPercentageQuestion(int percent)
        {
            double minValue = 75.26548195478186;

            // Tính giá trị tương ứng với phần trăm
            double value = minValue - (0.75757575757 * percent);

            return value; // Làm tròn đến 3 chữ số sau dấu phẩy
        }
    }
}
