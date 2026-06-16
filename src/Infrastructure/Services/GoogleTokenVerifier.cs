using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nexus.User.Application;

namespace Nexus.User.Infrastructure.Services;

public sealed class GoogleTokenVerifier : IGoogleTokenVerifier
{
    private readonly HttpClient _httpClient;
    private readonly AuthOptions _options;
    private readonly ILogger<GoogleTokenVerifier> _logger;

    public GoogleTokenVerifier(HttpClient httpClient, IOptions<AuthOptions> options, ILogger<GoogleTokenVerifier> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<OAuthTokenPayload> VerifyAsync(string idToken, CancellationToken cancellationToken = default)
    {
        var requestUri = $"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(idToken)}";
        var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Google token verification returned {StatusCode}.", response.StatusCode);
            throw new InvalidOperationException("Invalid Google token.");
        }

        var payload = await response.Content.ReadFromJsonAsync<GoogleTokenInfo>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Invalid Google token.");

        if (!string.Equals(payload.Audience, _options.ClientId, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(payload.AuthorizedParty, _options.ClientId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Google token audience mismatch.");
        }

        if (payload.ExpiresAt < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Google token is expired.");
        }

        return new OAuthTokenPayload
        {
            ProviderId = payload.Subject,
            Email = payload.Email,
            EmailVerified = payload.EmailVerified,
            FullName = payload.Name
        };
    }

    private sealed class GoogleTokenInfo
    {
        [JsonPropertyName("sub")] public string Subject { get; set; } = string.Empty;
        [JsonPropertyName("aud")] public string Audience { get; set; } = string.Empty;
        [JsonPropertyName("azp")] public string AuthorizedParty { get; set; } = string.Empty;
        [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
        [JsonPropertyName("email_verified")] public bool EmailVerified { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("exp")] public long ExpiresAtUnix { get; set; }
        public DateTime ExpiresAt => DateTimeOffset.FromUnixTimeSeconds(ExpiresAtUnix).UtcDateTime;
    }
}
