using MediatR;
using NavigationPlatform.Identity.Application.Abstractions.Authentication;
using NavigationPlatform.Identity.Application.Commands.LogoutUser;
using NavigationPlatform.Identity.Domain.Enums;
using NavigationPlatform.Shared.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.Identity.Application.Commands.ChangeUserStatus;

public record ChangeUserStatusCommand(Guid Id, UserStatus Status) : IRequest
{

}
