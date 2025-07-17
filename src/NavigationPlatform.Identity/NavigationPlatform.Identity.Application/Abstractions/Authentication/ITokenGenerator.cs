using NavigationPlatform.Identity.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.Identity.Application.Abstractions.Authentication;

public interface ITokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);
    string GenerateRefreshToken(int size = 64);
}
