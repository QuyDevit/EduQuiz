namespace EduQuiz.Services
{
    public interface IEmailService
    {
        void SendEmail(string recipientEmail, string subject, string message);
    }
}
