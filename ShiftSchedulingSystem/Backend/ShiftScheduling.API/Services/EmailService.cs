using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace ShiftScheduling.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string toName, string role, string temporaryPassword)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = emailSettings["SmtpServer"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"]);
                var senderEmail = emailSettings["SenderEmail"];
                var senderName = emailSettings["SenderName"];
                var senderPassword = emailSettings["SenderPassword"];
                var enableSsl = bool.Parse(emailSettings["EnableSsl"]);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = $"Welcome to Shift Scheduling System - {role} Account Created";

                var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";
                var loginLink = $"{frontendUrl}/login";

                var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            border: 1px solid #ddd;
            border-radius: 10px;
        }}
        .header {{
            background-color: #2563eb;
            color: white;
            padding: 20px;
            text-align: center;
            border-radius: 10px 10px 0 0;
        }}
        .content {{
            padding: 20px;
        }}
        .credentials {{
            background-color: #f3f4f6;
            padding: 15px;
            border-radius: 5px;
            margin: 20px 0;
        }}
        .footer {{
            text-align: center;
            padding: 20px;
            font-size: 12px;
            color: #666;
            border-top: 1px solid #ddd;
        }}
        .button {{
            display: inline-block;
            padding: 10px 20px;
            background-color: #2563eb;
            color: white;
            text-decoration: none;
            border-radius: 5px;
            margin-top: 20px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Welcome to Shift Scheduling System</h2>
        </div>
        <div class='content'>
            <p>Dear {toName},</p>
            <p>Your account has been created as a <strong>{role}</strong> in the Shift Scheduling System.</p>
            
            <div class='credentials'>
                <h3>Your Login Credentials:</h3>
                <p><strong>Email:</strong> {toEmail}</p>
                <p><strong>Temporary Password:</strong> {temporaryPassword}</p>
            </div>
            
            <p>Please click the button below to log in and change your password:</p>
            <p style='text-align: center;'>
                <a href='{loginLink}' class='button'>Login to Your Account</a>
            </p>
            
            <p><strong>Security Tips:</strong></p>
            <ul>
                <li>Change your password immediately after first login</li>
                <li>Do not share your password with anyone</li>
                <li>Contact your administrator if you didn't request this account</li>
            </ul>
        </div>
        <div class='footer'>
            <p>This is an automated message, please do not reply to this email.</p>
            <p>&copy; 2024 Shift Scheduling System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

                message.Body = new TextPart("html") { Text = body };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtpServer, smtpPort, enableSsl);
                    await client.AuthenticateAsync(senderEmail, senderPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation($"Welcome email sent to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send welcome email to {toEmail}");
                throw;
            }
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string toName, string newPassword)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = emailSettings["SmtpServer"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"]);
                var senderEmail = emailSettings["SenderEmail"];
                var senderName = emailSettings["SenderName"];
                var senderPassword = emailSettings["SenderPassword"];
                var enableSsl = bool.Parse(emailSettings["EnableSsl"]);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = "Password Reset - Shift Scheduling System";

                var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";
                var loginLink = $"{frontendUrl}/login";

                var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            border: 1px solid #ddd;
            border-radius: 10px;
        }}
        .header {{
            background-color: #2563eb;
            color: white;
            padding: 20px;
            text-align: center;
            border-radius: 10px 10px 0 0;
        }}
        .content {{
            padding: 20px;
        }}
        .credentials {{
            background-color: #f3f4f6;
            padding: 15px;
            border-radius: 5px;
            margin: 20px 0;
        }}
        .footer {{
            text-align: center;
            padding: 20px;
            font-size: 12px;
            color: #666;
            border-top: 1px solid #ddd;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Password Reset</h2>
        </div>
        <div class='content'>
            <p>Dear {toName},</p>
            <p>Your password has been reset by an administrator.</p>
            
            <div class='credentials'>
                <h3>Your New Password:</h3>
                <p><strong>New Password:</strong> {newPassword}</p>
            </div>
            
            <p>Please log in using the link below and change your password immediately:</p>
            <p style='text-align: center;'>
                <a href='{loginLink}' style='display: inline-block; padding: 10px 20px; background-color: #2563eb; color: white; text-decoration: none; border-radius: 5px;'>Login Here</a>
            </p>
        </div>
        <div class='footer'>
            <p>This is an automated message, please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";

                message.Body = new TextPart("html") { Text = body };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtpServer, smtpPort, enableSsl);
                    await client.AuthenticateAsync(senderEmail, senderPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation($"Password reset email sent to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send password reset email to {toEmail}");
                throw;
            }
        }
    }
}
