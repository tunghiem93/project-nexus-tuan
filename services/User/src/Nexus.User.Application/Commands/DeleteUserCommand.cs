using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Nexus.User.Application.Commands;

public sealed record DeleteUserCommand(Guid UserId)
    : IRequest;
