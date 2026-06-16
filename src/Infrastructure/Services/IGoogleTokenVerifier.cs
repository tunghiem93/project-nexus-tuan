using System.Threading;
using System.Threading.Tasks;

namespace Nexus.User.Infrastructure.Services;

public interface IGoogleTokenVerifier
{
    Task<OAuthTokenPayload> VerifyAsync(string idToken, CancellationToken cancellationToken = default);
}

public sealed class OAuthTokenPayload
{
    public string ProviderId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool EmailVerified { get; init; }
    public string FullName { get; init; } = string.Empty;
}
