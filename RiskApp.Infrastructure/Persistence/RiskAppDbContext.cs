using Microsoft.EntityFrameworkCore;
using RiskApp.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using RiskApp.Infrastructure.Users;

namespace RiskApp.Infrastructure.Persistence;

public class RiskAppDbContext : IdentityDbContext<AppUser>
{
    public RiskAppDbContext(DbContextOptions<RiskAppDbContext> options) : base(options) { }

    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<EmploymentRecord> EmploymentRecords => Set<EmploymentRecord>();
    public DbSet<Earning> Earnings => Set<Earning>();
    public DbSet<RiskAssessment> RiskAssessments => Set<RiskAssessment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RiskAppDbContext).Assembly);
        base.OnModelCreating(modelBuilder); ; // IMPORTANT: ensures Identity tables
    }
}
