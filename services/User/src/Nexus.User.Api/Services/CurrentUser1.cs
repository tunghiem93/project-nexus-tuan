using System.Security.Claims;
using Nexus.User.Domain.Entities;
namespace Nexus.User.Api.Services;

public class CurrentUser1 : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser1(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId =>
        Guid.Parse(
            _httpContextAccessor.HttpContext!
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

    public IReadOnlyCollection<string> Roles =>
        _httpContextAccessor.HttpContext!
            .User
            .FindAll(ClaimTypes.Role)
            .Select(x => x.Value)
            .ToList();
}
