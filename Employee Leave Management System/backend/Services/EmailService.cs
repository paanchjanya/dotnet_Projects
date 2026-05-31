using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EmployeeLeaveApi.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string toEmail, string subject, string body)
        {
            _logger.LogInformation("\n" +
                "============================================================\n" +
                "SIMULATED EMAIL SENT\n" +
                "To: {ToEmail}\n" +
                "Subject: {Subject}\n" +
                "Body:\n{Body}\n" +
                "============================================================",
                toEmail, subject, body);

            return Task.CompletedTask;
        }
    }
}
