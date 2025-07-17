using MediatR;
using NavigationPlatform.Identity.Application.Commands.LoginUser;
using NavigationPlatform.Identity.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.Identity.Application.Commands.RegisterUser;

public record RegisterUserCommand(
string Email,
string Password,
UserRole Role
) : IRequest<AuthResult>;
