namespace ShiftScheduling.API.Services
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string toEmail, string toName, string role, string temporaryPassword);
        Task SendPasswordResetEmailAsync(string toEmail, string toName, string newPassword);
    }
}