using Grpc.Core;
using Nexus.User.Api.Grpc;
using Nexus.User.Application.Services;
using Nexus.User.Contracts.Dtos;

namespace Nexus.User.Api.Grpc;

public class UserGrpcService : UserGrpc.UserGrpcBase
{
    private readonly IUserQueryService _userQueryService;
    private readonly IJwtTokenService _jwtTokenService;

    public UserGrpcService(IUserQueryService userQueryService, IJwtTokenService jwtTokenService)
    {
        _userQueryService = userQueryService;
        _jwtTokenService = jwtTokenService;
    }

    public override async Task<GetUserByIdResponse> GetUserById(GetUserByIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var userId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user id."));
        }

        var user = await _userQueryService.GetByIdAsync(userId, context.CancellationToken);
        if (user is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "User not found."));
        }

        return new GetUserByIdResponse
        {
            Id = user.Id.ToString(),
            Email = user.Email,
            FullName = user.FullName,
            Status = user.Status
        };
    }

    public override Task<ValidateTokenResponse> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
    {
        try
        {
            var principal = _jwtTokenService.ValidateAccessToken(request.AccessToken);
            var subject = principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value ?? string.Empty;
            var role = principal.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;
            var scopes = principal.FindAll("scope").Select(c => c.Value);

            return Task.FromResult(new ValidateTokenResponse
            {
                Valid = true,
                UserId = subject,
                Role = role,
                Scope = { scopes }
            });
        }
        catch
        {
            return Task.FromResult(new ValidateTokenResponse { Valid = false });
        }
    }
}
