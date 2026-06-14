using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.User.Domain.Entities;

public interface ICurrentUser
{
    Guid UserId { get; }

    IReadOnlyCollection<string> Roles { get; }
}
