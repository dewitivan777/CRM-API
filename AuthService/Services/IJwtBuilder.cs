using System;

namespace AuthService.Services
{
    public interface IJwtBuilder
    {
        string GetToken(string userId);
        string ValidateToken(string token);
    }
}
