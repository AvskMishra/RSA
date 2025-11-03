using Microsoft.EntityFrameworkCore;
using RiskApp.Domain.Entities;

namespace RiskApp.Infrastructure.Persistence;

public class RiskAppDbContext : DbContext
{
    public RiskAppDbContext(DbContextOptions<RiskAppDbContext> options) : base(options) { }

    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<EmploymentRecord> EmploymentRecords => Set<EmploymentRecord>();
    public DbSet<Earning> Earnings => Set<Earning>();
    public DbSet<RiskAssessment> RiskAssessments => Set<RiskAssessment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RiskAppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
