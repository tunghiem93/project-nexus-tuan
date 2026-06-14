using Nexus.User.Application.Dtos;

namespace Nexus.User.Application.Services;

public interface IUserQueryService
{
    Task<UserSummaryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
