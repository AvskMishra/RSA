using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RiskApp.Domain.Entities;

namespace RiskApp.Infrastructure.Persistence.Configurations;

public class ProfileConfig : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> b)
    {
        b.ToTable("profiles");
        b.HasKey(x => x.Id);

        b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        b.Property(x => x.NationalId).HasMaxLength(50).IsRequired();
        b.Property(x => x.Email).HasMaxLength(200);
        b.Property(x => x.Phone).HasMaxLength(30);
        b.Property(x => x.Address).HasMaxLength(500);

        b.HasIndex(x => x.NationalId).IsUnique();

        b.HasMany(x => x.EmploymentHistory)
         .WithOne(x => x.Profile!)
         .HasForeignKey(x => x.ProfileId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(x => x.Earnings)
         .WithOne(x => x.Profile!)
         .HasForeignKey(x => x.ProfileId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
