using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RiskApp.Domain.Entities;

namespace RiskApp.Infrastructure.Persistence.Configurations;

public class RiskAssessmentConfig : IEntityTypeConfiguration<RiskAssessment>
{
    public void Configure(EntityTypeBuilder<RiskAssessment> b)
    {
        b.ToTable("risk_assessments");
        b.HasKey(x => x.Id);
        b.Property(x => x.Recommendations).HasMaxLength(2000);
    }
}
