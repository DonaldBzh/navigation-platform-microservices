using NavigationPlatform.UserManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.UserManagement.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get;  set; } = null!;
    public UserStatus Status { get;  set; }
    public UserRole Role { get;  set; }
    public DateTime CreatedAt { get;  set; }

    public DateTime? UpdatedAt { get; set; }


    public void UpdateStatus(UserStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

}
