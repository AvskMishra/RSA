using Microsoft.EntityFrameworkCore;
using RiskApp.Domain.Entities;
using RiskApp.Domain.Enums;

namespace RiskApp.Infrastructure.Persistence;

public static class SeedData
{
    // Call this first: inserts 2 Profiles, 2 EmploymentRecords, 2 Earnings
    public static async Task EnsureSeededAsync(RiskAppDbContext db, CancellationToken ct = default)
    {
        #region base data seed
        // Idempotency: if any profile exists, assume already seeded
        if (await db.Profiles.AsNoTracking().AnyAsync(ct)) return;

        // Deterministic IDs (optional)
        var profile1Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var profile2Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var emp1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var emp2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var earn1Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var earn2Id = Guid.Parse("44444444-4444-4444-4444-444444444444");

        // --- Profiles (2) ---
        var p1 = new Profile(profile1Id, "Amit Sharma", new DateOnly(1990, 3, 21),
                             "PANAS1234X", "amit.sharma@example.com", "+91-9000000001", "Bengaluru, KA");
        var p2 = new Profile(profile2Id, "Riya Sen", new DateOnly(1993, 5, 12),
                             "PANRS5678Y", "riya.sen@example.com", "+91-9000000002", "Kolkata, WB");
        db.Profiles.AddRange(p1, p2);

        // --- EmploymentRecords (2) ---
        var eWork1 = new EmploymentRecord(emp1Id, profile1Id, "Innotech Pvt Ltd",
                        EmploymentType.FullTime, new DateOnly(2022, 1, 1), 60000m);
        var eWork2 = new EmploymentRecord(emp2Id, profile2Id, "Freelance",
                        EmploymentType.SelfEmployed, new DateOnly(2023, 6, 1), 45000m);
        db.EmploymentRecords.AddRange(eWork1, eWork2);

        // --- Earnings (2) ---
        var earn1 = new Earning(earn1Id, profile1Id, 60000m, 5000m, new DateOnly(2024, 10, 1), "INR");
        var earn2 = new Earning(earn2Id, profile2Id, 45000m, 10000m, new DateOnly(2024, 9, 1), "INR");
        db.Earnings.AddRange(earn1, earn2);

        await db.SaveChangesAsync(ct);

        #endregion
    }

    // Optional: create 2 RiskAssessment rows via the real service
    public static async Task EnsureRiskAssessmentsSeededAsync( RiskAppDbContext db,Application.Risk.IRiskAssessmentService riskService,CancellationToken ct = default)
    {
        if (await db.RiskAssessments.AsNoTracking().AnyAsync(ct)) return;

        var profileIds = await db.Profiles.AsNoTracking()
            .OrderBy(p => p.CreatedAtUtc)
            .Select(p => p.Id)
            .Take(2)
            .ToListAsync(ct);

        foreach (var pid in profileIds)
        {
            await riskService.AssessAsync(new Application.Risk.RiskAssessRequestDto { ProfileId = pid }, ct);
        }
    }
}
