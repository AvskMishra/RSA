using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RiskApp.Domain.Entities;

namespace RiskApp.Infrastructure.Persistence.Configurations;

public class EarningConfig : IEntityTypeConfiguration<Earning>
{
    public void Configure(EntityTypeBuilder<Earning> b)
    {
        b.ToTable("earnings");
        b.HasKey(x => x.Id);

        b.Property(x => x.MonthlyIncome).HasPrecision(18, 2);
        b.Property(x => x.OtherMonthlyIncome).HasPrecision(18, 2);
        b.Property(x => x.Currency).HasMaxLength(10).HasDefaultValue("INR");
    }
}
