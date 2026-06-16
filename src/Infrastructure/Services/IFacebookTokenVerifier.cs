using System.Threading;
using System.Threading.Tasks;

namespace Nexus.User.Infrastructure.Services;

public interface IFacebookTokenVerifier
{
    Task<OAuthTokenPayload> VerifyAsync(string idToken, CancellationToken cancellationToken = default);
}
