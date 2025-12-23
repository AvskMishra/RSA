using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RiskApp.Infrastructure.Users;

namespace RiskApp.Infrastructure.Auth;

public static class IdentitySeed
{
    private sealed class SeedUser
    {
        public string Email { get; set; } = default!;
        public string? DisplayName { get; set; }
        public string Role { get; set; } = "Reader";
        public string? Password { get; set; } // if null, use DefaultPassword
    }

    public static async Task EnsureSeededAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("IdentitySeed");
        var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        // ---- config ----
        var section = cfg.GetSection("AuthSeeding");
        var enabled = section.GetValue("Enabled", false);
        var defaultPassword = section.GetValue<string>("DefaultPassword") ?? "Demo#123";
        var roles = section.GetSection("Roles").Get<string[]>() ?? new[] { "Reader", "Writer" };
        var users = section.GetSection("Users").Get<List<SeedUser>>() ??
                new()
                {
                    new SeedUser { Email="reader@demo.local", DisplayName="reader", Role="Reader" },
                    new SeedUser { Email="writer@demo.local", DisplayName="writer", Role="Writer" }
                };

        if (!enabled)
        {
            logger.LogInformation("Auth seeding disabled via configuration.");
            return;
        }

        logger.LogInformation("Starting Identity seeding… Roles={RolesCount} Users={UsersCount}", roles.Length, users.Count);

        // ---- roles (idempotent) ----
        foreach (var role in roles.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (await roleMgr.RoleExistsAsync(role))
            {
                logger.LogInformation("Role exists: {Role}", role);
            }
            else
            {
                var created = await roleMgr.CreateAsync(new IdentityRole(role));
                if (created.Succeeded)
                    logger.LogInformation("Role created: {Role}", role);
                else
                    logger.LogWarning("Failed creating role {Role}: {Errors}", role,
                        string.Join("; ", created.Errors.Select(e => $"{e.Code}:{e.Description}")));
            }
        }

        // ---- users (idempotent) ----
        foreach (var u in users)
        {
            if (string.IsNullOrWhiteSpace(u.Email))
            {
                logger.LogWarning("Skipping user with empty email in config.");
                continue;
            }
            var role = string.IsNullOrWhiteSpace(u.Role) ? "Reader" : u.Role;
            var pwd = u.Password ?? defaultPassword;

            var user = await userMgr.FindByEmailAsync(u.Email);
            if (user is null)
            {
                user = new AppUser { UserName = u.Email, Email = u.Email, DisplayName = u.DisplayName ?? u.Email };
                var created = await userMgr.CreateAsync(user, pwd);
                if (created.Succeeded)
                    logger.LogInformation("User created: {Email}", u.Email);
                else
                {
                    logger.LogWarning("Failed creating user {Email}: {Errors}", u.Email,
                        string.Join("; ", created.Errors.Select(e => $"{e.Code}:{e.Description}")));
                    continue;
                }
            }
            else
            {
                logger.LogInformation("User exists: {Email}", u.Email);
            }

            // ensure role membership
            if (!await userMgr.IsInRoleAsync(user, role))
            {
                var added = await userMgr.AddToRoleAsync(user, role);
                if (added.Succeeded)
                    logger.LogInformation("User {Email} added to role {Role}", u.Email, role);
                else
                    logger.LogWarning("Failed to add user {Email} to role {Role}: {Errors}", u.Email, role,
                        string.Join("; ", added.Errors.Select(e => $"{e.Code}:{e.Description}")));
            }
            else
            {
                logger.LogInformation("User {Email} already in role {Role}", u.Email, role);
            }
        }

        logger.LogInformation("Identity seeding complete.");
    }
}
