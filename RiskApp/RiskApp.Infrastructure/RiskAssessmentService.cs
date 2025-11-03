using Microsoft.EntityFrameworkCore;
using RiskApp.Application.Risk;
using RiskApp.Application.Risk.Providers;
using RiskApp.Domain.Entities;
using RiskApp.Domain.Enums;
using RiskApp.Infrastructure.Persistence;

public class RiskAssessmentService : IRiskAssessmentService
{
    private readonly RiskAppDbContext _db;
    private readonly ICreditProvider _credit;
    private readonly IFraudProvider _fraud;

    public RiskAssessmentService(RiskAppDbContext db, ICreditProvider credit, IFraudProvider fraud)
    {
        _db = db;
        _credit = credit;
        _fraud = fraud;
    }
    public async Task<RiskAssessmentReadDto> AssessAsync(RiskAssessRequestDto request, CancellationToken ct = default)
    {
        var profile = await _db.Profiles
            .Include(p => p.EmploymentHistory)
            .Include(p => p.Earnings)
            .FirstOrDefaultAsync(p => p.Id == request.ProfileId, ct);

        if (profile is null)
            throw new InvalidOperationException("Profile not found.");

        // ------- existing feature extraction (unchanged) -------
        var currentEmployment = profile.EmploymentHistory.FirstOrDefault(e => e.IsCurrent);
        var yearsInCurrentJob = currentEmployment is null
            ? 0
            : Math.Max(0, (DateTime.UtcNow.Date - new DateTime(currentEmployment.StartDate.Year, currentEmployment.StartDate.Month, currentEmployment.StartDate.Day)).TotalDays / 365.25);

        var latestEarning = profile.Earnings.OrderByDescending(e => e.EffectiveFrom).FirstOrDefault();
        var totalMonthlyIncome = latestEarning is null ? 0m : latestEarning.MonthlyIncome + latestEarning.OtherMonthlyIncome;

        int score = 300;
        score += totalMonthlyIncome switch
        {
            >= 100_000m => 200,
            >= 50_000m => 150,
            >= 25_000m => 100,
            >= 15_000m => 70,
            _ => 40
        };
        score += yearsInCurrentJob switch
        {
            >= 3.0 => 150,
            >= 1.0 => 100,
            > 0.0 => 60,
            _ => 20
        };
        if (currentEmployment is null) score -= 40;
        if (latestEarning is null) score -= 40;

        // ------- NEW: external providers (optional) -------
        CreditScoreResult? credit = null;
        FraudSignals? fraud = null;

        if (request.UseExternalProviders)
        {
            // Call both in parallel for latency hiding
            var creditTask = _credit.GetCreditScoreAsync(profile.NationalId, ct);
            var fraudTask = _fraud.GetSignalsAsync(profile.NationalId, profile.Email, profile.Phone, ct);
            await Task.WhenAll(creditTask, fraudTask);
            credit = await creditTask;
            fraud = await fraudTask;

            // Blend credit score (normalize 300..900 -> 0..300 points)
            if (credit is not null)
            {
                var normalized = Math.Clamp(credit.Score, 300, 900) - 300; // 0..600
                var creditPoints = (int)Math.Round(normalized / 2.0);      // 0..300
                score += creditPoints;
            }

            // Penalize fraud risk (0..100 -> 0..200 negative points)
            if (fraud is not null)
            {
                var fraudPenalty = Math.Clamp(fraud.RiskLevel, 0, 100) * 2; // 0..200
                score -= fraudPenalty;

                // Hard gate examples (optional)
                if (fraud.IsHighRiskIdentity && fraud.RiskLevel >= 70)
                {
                    // ensure decision won't be Approve later
                    score = Math.Min(score, 599);
                }
            }
        }

        // Clamp and decide
        score = Math.Clamp(score, 0, 1000);
        var decision = score switch
        {
            >= 700 => RiskDecision.Approve,
            >= 600 => RiskDecision.Review,
            _ => RiskDecision.Decline
        };

        var recs = BuildRecommendations(score, totalMonthlyIncome, yearsInCurrentJob,
                                        currentEmployment is not null, latestEarning is not null,
                                        credit, fraud);

        var assessment = new RiskAssessment(profile.Id);
        assessment.RecordOutcome(score, decision, recs);

        _db.RiskAssessments.Add(assessment);
        await _db.SaveChangesAsync(ct);

        return Map(assessment);
    }
    public async Task<RiskAssessmentReadDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _db.RiskAssessments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return e is null ? null : Map(e);
    }
    public async Task<IReadOnlyList<RiskAssessmentReadDto>> ListByProfileAsync(Guid profileId, int skip = 0, int take = 50, CancellationToken ct = default)
    {
        return await _db.RiskAssessments
            .AsNoTracking()
            .Where(x => x.ProfileId == profileId)
            .OrderByDescending(x => x.AssessedOnUtc)
            .Skip(skip)
            .Take(take)
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
    private static string BuildRecommendations(int score, decimal income, double yearsInCurrent, bool hasEmployment, bool hasEarning, CreditScoreResult? credit, FraudSignals? fraud)
    {
        var tips = new List<string>();
        if (!hasEmployment) tips.Add("Add current employment or update employment status.");
        if (!hasEarning) tips.Add("Provide a recent earning snapshot.");

        if (income < 25000m) tips.Add("Consider increasing stable monthly income.");
        if (yearsInCurrent < 1.0) tips.Add("Longer job tenure can improve stability.");

        if (credit is not null) tips.Add($"Credit score from {credit.Provider}: {credit.Score}.");
        if (fraud is not null && fraud.RiskLevel > 0) tips.Add($"Fraud risk from {fraud.Provider}: {fraud.RiskLevel}/100.");

        if (score >= 700) tips.Add("Eligible for approval; maintain income stability.");
        else if (score >= 600) tips.Add("Borderline: submit additional proofs for credit/income.");
        else tips.Add("High risk: improve stability and reassess.");

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
