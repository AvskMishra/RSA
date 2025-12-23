using Microsoft.AspNetCore.Identity;

namespace RiskApp.Infrastructure.Users;

public class AppUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
