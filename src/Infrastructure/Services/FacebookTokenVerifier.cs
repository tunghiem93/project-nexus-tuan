using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nexus.User.Application;

namespace Nexus.User.Infrastructure.Services;

public sealed class FacebookTokenVerifier : IFacebookTokenVerifier
{
    private readonly HttpClient _httpClient;
    private readonly AuthOptions _options;
    private readonly ILogger<FacebookTokenVerifier> _logger;

    public FacebookTokenVerifier(HttpClient httpClient, IOptions<AuthOptions> options, ILogger<FacebookTokenVerifier> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<OAuthTokenPayload> VerifyAsync(string idToken, CancellationToken cancellationToken = default)
    {
        var debugUri = $"https://graph.facebook.com/debug_token?input_token={Uri.EscapeDataString(idToken)}&access_token={Uri.EscapeDataString(_options.FacebookAppToken ?? string.Empty)}";
        var debugResponse = await _httpClient.GetAsync(debugUri, cancellationToken);
        if (!debugResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning("Facebook token debug returned {StatusCode}.", debugResponse.StatusCode);
            throw new InvalidOperationException("Invalid Facebook token.");
        }

        var debugPayload = await debugResponse.Content.ReadFromJsonAsync<FacebookDebugTokenResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Invalid Facebook token.");

        if (!debugPayload.Data.IsValid || debugPayload.Data.AppId != _options.FacebookAppId)
        {
            throw new InvalidOperationException("Invalid Facebook token.");
        }

        var userInfoUri = $"https://graph.facebook.com/me?fields=id,name,email&access_token={Uri.EscapeDataString(idToken)}";
        var userInfo = await _httpClient.GetFromJsonAsync<FacebookUserInfo>(userInfoUri, cancellationToken)
            ?? throw new InvalidOperationException("Unable to retrieve Facebook user info.");

        return new OAuthTokenPayload
        {
            ProviderId = userInfo.Id,
            Email = userInfo.Email,
            EmailVerified = !string.IsNullOrWhiteSpace(userInfo.Email),
            FullName = userInfo.Name
        };
    }

    private sealed class FacebookDebugTokenResponse
    {
        [JsonPropertyName("data")] public FacebookDebugData Data { get; set; } = new();
    }

    private sealed class FacebookDebugData
    {
        [JsonPropertyName("app_id")] public string AppId { get; set; } = string.Empty;
        [JsonPropertyName("is_valid")] public bool IsValid { get; set; }
    }

    private sealed class FacebookUserInfo
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
    }
}
