using MediatR;
using NavigationPlatform.UserManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.UserManagement.Application.Commands.UpdateUserStatus;

public record UpdateUserStatusCommand(Guid UserId, UserStatus Status,string? Reason) : IRequest<UserResponse>
{
}
