using EmployeeLeaveApi.Models;

namespace EmployeeLeaveApi.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
