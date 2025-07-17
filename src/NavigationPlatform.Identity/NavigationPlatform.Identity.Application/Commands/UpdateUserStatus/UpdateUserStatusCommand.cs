using MediatR;
using NavigationPlatform.Identity.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.Identity.Application.Commands.UpdateUserStatus;

public class UpdateUserStatusCommand : IRequest
{
    public Guid UserId { get; init; }
    public UserStatus OldStatus { get; init; }
    public UserStatus NewStatus { get; init; }
    public string Reason { get; init; } = null!;
    public Guid AdminUserId { get; init; }
}
