using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nexus.User.Application.Common.Interfaces;
using Nexus.User.Infrastructure.Persistence;
using Nexus.User.Domain.Entities;
using UserEntity = Nexus.User.Domain.Entities.User;

namespace Nexus.User.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _dbContext;

    public UserRepository(UserDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users1
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }
    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
