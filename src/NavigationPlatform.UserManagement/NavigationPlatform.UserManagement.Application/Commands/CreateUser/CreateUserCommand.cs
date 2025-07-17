using MediatR;
using NavigationPlatform.UserManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.UserManagement.Application.Commands.CreateUser;

public class CreateUserCommand : IRequest
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public UserStatus Status { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
}
