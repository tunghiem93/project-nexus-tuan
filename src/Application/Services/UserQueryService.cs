using Microsoft.EntityFrameworkCore;
using Nexus.Abstractions.Persistence;
using Nexus.User.Application.Dtos;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Application.Services;

public class UserQueryService(IRepository<UserAccount> userRepository) : IUserQueryService
{
    public async Task<UserSummaryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);

        return user is null
            ? null
            : new UserSummaryDto(user.Id, user.Email, user.FullName, user.Status);
    }
}
