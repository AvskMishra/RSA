using Microsoft.EntityFrameworkCore;
using RiskApp.Application.Risk;
using RiskApp.Domain.Entities;
using RiskApp.Domain.Enums;
using RiskApp.Infrastructure.Persistence;

namespace RiskApp.Infrastructure.Risk;

public class RiskAssessmentService : IRiskAssessmentService
{
    private readonly RiskAppDbContext _db;

    public RiskAssessmentService(RiskAppDbContext db) => _db = db;

    public async Task<RiskAssessmentReadDto> AssessAsync(RiskAssessRequestDto request, CancellationToken ct = default)
    {
        // 1) Load the inputs we need (single round-trip)
        var profile = await _db.Profiles
            .Include(p => p.EmploymentHistory)
            .Include(p => p.Earnings)
            .FirstOrDefaultAsync(p => p.Id == request.ProfileId, ct);

        if (profile is null)
            throw new InvalidOperationException("Profile not found.");

        // 2) Extract features
        var currentEmployment = profile.EmploymentHistory.FirstOrDefault(e => e.IsCurrent);
        var yearsInCurrentJob = currentEmployment is null
            ? 0
            : Math.Max(0, (DateTime.UtcNow.Date - new DateTime(currentEmployment.StartDate.Year, currentEmployment.StartDate.Month, currentEmployment.StartDate.Day)).TotalDays / 365.25);

        var latestEarning = profile.Earnings.OrderByDescending(e => e.EffectiveFrom).FirstOrDefault();
        var totalMonthlyIncome = latestEarning is null ? 0m : latestEarning.MonthlyIncome + latestEarning.OtherMonthlyIncome;

        // 3) Scorecard v0 (simple, transparent)
        //    Range target ~ 300..850 (loosely credit-score shaped). Clamp 0..1000 for safety.
        int score = 300;

        // Income bands
        score += totalMonthlyIncome switch
        {
            >= 100_000m => 200,
            >= 50_000m => 150,
            >= 25_000m => 100,
            >= 15_000m => 70,
            _ => 40
        };

        // Employment stability
        score += yearsInCurrentJob switch
        {
            >= 3.0 => 150,
            >= 1.0 => 100,
            > 0.0 => 60,
            _ => 20 // no current job
        };

        // Minimal penalties if no data
        if (currentEmployment is null) score -= 40;
        if (latestEarning is null) score -= 40;

        // (No external credit/fraud yet; we’ll add providers later)

        // Clamp and map decision
        score = Math.Clamp(score, 0, 1000);

        var decision = score switch
        {
            >= 700 => RiskDecision.Approve,
            >= 600 => RiskDecision.Review,
            _ => RiskDecision.Decline
        };

        // Recommendations (brief)
        var recs = BuildRecommendations(score, totalMonthlyIncome, yearsInCurrentJob, currentEmployment is not null, latestEarning is not null);

        // 4) Persist assessment
        var assessment = new RiskAssessment(profile.Id);
        assessment.RecordOutcome(score, decision, recs);

        _db.RiskAssessments.Add(assessment);
        await _db.SaveChangesAsync(ct);

        return Map(assessment);
    }

    public async Task<RiskAssessmentReadDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _db.RiskAssessments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? null : Map(e);
    }

    public async Task<IReadOnlyList<RiskAssessmentReadDto>> ListByProfileAsync(Guid profileId, int skip = 0, int take = 50, CancellationToken ct = default)
    {
        return await _db.RiskAssessments.AsNoTracking()
            .Where(x => x.ProfileId == profileId)
            .OrderByDescending(x => x.AssessedOnUtc)
            .Skip(skip).Take(take)
            .Select(x => new RiskAssessmentReadDto
            {
                Id = x.Id,
                ProfileId = x.ProfileId,
                AssessedOnUtc = x.AssessedOnUtc,
                Score = x.Score,
                Decision = x.Decision,
                Recommendations = x.Recommendations
            })
            .ToListAsync(ct);
    }

    private static string BuildRecommendations(int score, decimal income, double yearsInCurrent, bool hasEmployment, bool hasEarning)
    {
        var tips = new List<string>();

        if (!hasEmployment) tips.Add("Add current employment or update employment status.");
        if (!hasEarning) tips.Add("Provide a recent earning snapshot.");
        if (income < 25000m) tips.Add("Consider increasing stable monthly income or reducing fixed expenses.");
        if (yearsInCurrent < 1.0) tips.Add("Longer tenure at current job can improve stability.");

        if (score >= 700) tips.Add("Eligible for approval; maintain income stability.");
        else if (score >= 600) tips.Add("Borderline: submit additional income proofs or employment history.");
        else tips.Add("High risk: improve income stability and tenure, then reassess.");

        return string.Join(" ", tips);
    }

    private static RiskAssessmentReadDto Map(RiskAssessment x) => new()
    {
        Id = x.Id,
        ProfileId = x.ProfileId,
        AssessedOnUtc = x.AssessedOnUtc,
        Score = x.Score,
        Decision = x.Decision,
        Recommendations = x.Recommendations
    };
}
