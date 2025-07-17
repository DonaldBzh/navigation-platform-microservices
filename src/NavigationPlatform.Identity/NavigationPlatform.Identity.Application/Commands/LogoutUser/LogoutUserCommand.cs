using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.Identity.Application.Commands.LogoutUser;

public record LogoutUserCommand(string RefreshToken) : IRequest;
