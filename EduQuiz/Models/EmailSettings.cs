namespace EduQuiz.Models
{
    public class EmailSettings
    {
        public string SMTPHost { get; set; }
        public int SMTPPort { get; set; }
        public string SMTPUser { get; set; }
        public string SMTPPassword { get; set; }
        public bool EnableSSL { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
    }
}
