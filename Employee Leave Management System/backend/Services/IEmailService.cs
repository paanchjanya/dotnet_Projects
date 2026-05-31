using System.Threading.Tasks;

namespace EmployeeLeaveApi.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
