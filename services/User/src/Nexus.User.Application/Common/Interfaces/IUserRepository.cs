using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserEntity = Nexus.User.Domain.Entities.User;
namespace Nexus.User.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<UserEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(UserEntity? user);
}
