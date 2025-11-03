using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RiskApp.Domain.Entities;

namespace RiskApp.Infrastructure.Persistence.Configurations;

public class EmploymentRecordConfig : IEntityTypeConfiguration<EmploymentRecord>
{
    public void Configure(EntityTypeBuilder<EmploymentRecord> b)
    {
        b.ToTable("employment_records");
        b.HasKey(x => x.Id);

        b.Property(x => x.EmployerName).HasMaxLength(200).IsRequired();
        b.Property(x => x.MonthlyIncome).HasPrecision(18, 2);
    }
}
