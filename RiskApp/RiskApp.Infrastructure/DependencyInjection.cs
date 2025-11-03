using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RiskApp.Application.Abstractions;
using RiskApp.Application.Analytics;
using RiskApp.Application.Earnings;
using RiskApp.Application.Profiles;
using RiskApp.Application.Risk;
using RiskApp.Application.Risk.Providers;
using RiskApp.Application.Work;
using RiskApp.Infrastructure.Analytics;
using RiskApp.Infrastructure.Earnings;
using RiskApp.Infrastructure.Persistence;
using RiskApp.Infrastructure.Profiles;
using RiskApp.Infrastructure.Risk;
using RiskApp.Infrastructure.Risk.Providers;
using RiskApp.Infrastructure.Work;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RiskApp.Application.Auth;
using RiskApp.Infrastructure.Auth;
using Microsoft.Extensions.Configuration;
using RiskApp.Infrastructure.Users;

namespace RiskApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string sqliteConnection)
    {
        //db context
        services.AddDbContext<RiskAppDbContext>(opt =>
        {
            opt.UseSqlite(sqliteConnection);
            opt.EnableDetailedErrors();
        });


        // ASP.NET Core Identity
        services.AddIdentity<AppUser, IdentityRole>(opt =>
        {
            opt.Password.RequireDigit = true;
            opt.Password.RequiredLength = 6;
            opt.Password.RequireUppercase = false;
            opt.Password.RequireNonAlphanumeric = false;
        })
        .AddEntityFrameworkStores<RiskAppDbContext>()
        .AddDefaultTokenProviders();

        // JWT Bearer
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var cfg = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = cfg["Jwt:Issuer"],
                    ValidAudience = cfg["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Key"]!))
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("CanRead", p => p.RequireRole("Reader", "Writer"));
            options.AddPolicy("CanWrite", p => p.RequireRole("Writer"));
        });

        // Auth abstractions
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthProvider, IdentityAuthProvider>();

        // Generic repo + UoW
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IReadOnlyRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IEmploymentService, EmploymentService>();
        services.AddScoped<IEarningService, EarningService>();
        services.AddScoped<IRiskAssessmentService, RiskAssessmentService>();

        // NEW: External providers (mock; swap to real later)
        services.AddScoped<ICreditProvider, MockCreditProvider>();
        services.AddScoped<IFraudProvider, MockFraudProvider>();

        services.AddScoped<IAnalyticsService, AnalyticsService>();
        return services;
    }
}
