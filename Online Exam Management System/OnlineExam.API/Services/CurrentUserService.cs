using System.Security.Claims;

namespace OnlineExam.API.Services;

public sealed class CurrentUserService(IHttpContextAccessor accessor)
{
    public int UserId
    {
        get
        {
            var value = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : 0;
        }
    }
}
