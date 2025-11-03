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

namespace RiskApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string sqliteConnection)
    {
        services.AddDbContext<RiskAppDbContext>(opt =>
        {
            opt.UseSqlite(sqliteConnection);
        });

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
