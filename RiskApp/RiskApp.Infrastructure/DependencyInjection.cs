using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RiskApp.Application.Earnings;
using RiskApp.Application.Profiles;
using RiskApp.Application.Risk;
using RiskApp.Application.Work;
using RiskApp.Infrastructure.Earnings;
using RiskApp.Infrastructure.Persistence;
using RiskApp.Infrastructure.Profiles;
using RiskApp.Infrastructure.Risk;
using RiskApp.Infrastructure.Work;

namespace RiskApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string sqliteConnection)
    {
        services.AddDbContext<RiskAppDbContext>(opt =>
        {
            opt.UseSqlite(sqliteConnection);
        });
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IEmploymentService, EmploymentService>();
        services.AddScoped<IEarningService, EarningService>();
        services.AddScoped<IRiskAssessmentService, RiskAssessmentService>();
        return services;
    }
}
