using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RiskApp.Application.Messaging;
using RiskApp.Application.Risk;
using RiskApp.Domain.Entities;
using RiskApp.Domain.Enums;
using RiskApp.Infrastructure.Persistence;
using MassTransit;

namespace RiskApp.Infrastructure.Risk;

public class RiskAssessmentService : IRiskAssessmentService
{
    private readonly RiskAppDbContext _db;
    private readonly ILogger<RiskAssessmentService> _logger;
    private readonly IRequestClient<PerformExternalChecks>? _externalChecksClient;

    public RiskAssessmentService(RiskAppDbContext db, ILogger<RiskAssessmentService> logger
        , IRequestClient<PerformExternalChecks>? externalChecksClient)
    {
        _db = db;
        _logger = logger;
        _externalChecksClient = externalChecksClient;
    }



   
    public async Task<RiskAssessmentReadDto> AssessAsync(RiskAssessRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Assessing risk for ProfileId={ProfileId}, UseExternal={UseExternal}",
            request.ProfileId, request.UseExternalProviders);

        // 1) Load the inputs we need (single round-trip)
        var profile = await _db.Profiles
            .Include(p => p.EmploymentHistory)
            .Include(p => p.Earnings)
            .FirstOrDefaultAsync(p => p.Id == request.ProfileId, ct);

        if (profile is null)
        {
            _logger.LogWarning("ProfileId {ProfileId} not found for risk assessment", request.ProfileId);
            throw new InvalidOperationException("Profile not found.");
        }

        // 2) Extract features
        var currentEmployment = profile.EmploymentHistory.FirstOrDefault(e => e.IsCurrent);
        _logger.LogInformation("Current employment found: {HasCurrentEmployment}", currentEmployment is not null);

        var yearsInCurrentJob = currentEmployment is null
            ? 0
            : Math.Max(0,
                (DateTime.UtcNow.Date - new DateTime(currentEmployment.StartDate.Year,
                                                     currentEmployment.StartDate.Month,
                                                     currentEmployment.StartDate.Day)).TotalDays / 365.25);

        var latestEarning = profile.Earnings.OrderByDescending(e => e.EffectiveFrom).FirstOrDefault();
        _logger.LogInformation("Latest earning found: {HasLatestEarning}", latestEarning is not null);

        var totalMonthlyIncome = latestEarning is null
            ? 0m
            : latestEarning.MonthlyIncome + latestEarning.OtherMonthlyIncome;
        _logger.LogInformation("Total monthly income calculated: {TotalMonthlyIncome}", totalMonthlyIncome);

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

        // ------------------------------------------
        // NEW: Optional external checks via MassTransit
        // ------------------------------------------
        int? creditScore = null;
        int? fraudRisk = null;
        bool? isHighRiskId = null;
        bool? emailWatchlist = null;
        bool? phoneWatchlist = null;

        if (request.UseExternalProviders && _externalChecksClient is not null)
        {
            try
            {
                var correlationId = Guid.NewGuid();
                _logger.LogInformation("Sending PerformExternalChecks: CorrelationId={CorrelationId}, ProfileId={ProfileId}",
                    correlationId, profile.Id);

                var response = await _externalChecksClient.GetResponse<ExternalChecksCompleted>(new
                {
                    CorrelationId = correlationId,
                    ProfileId = profile.Id,
                    NationalId = profile.NationalId,
                    Email = profile.Email,
                    Phone = profile.Phone
                }, ct);

                creditScore = response.Message.CreditScore;// 300..900 (example)
                fraudRisk = response.Message.FraudRiskLevel;// 0..100
                isHighRiskId = response.Message.IsHighRiskIdentity;
                emailWatchlist = response.Message.EmailOnWatchlist;
                phoneWatchlist = response.Message.PhoneOnWatchlist;

                _logger.LogInformation("External checks OK: Credit={Credit}, Fraud={Fraud}, HighRiskId={HighRiskId}, EmailWL={EmailWL}, PhoneWL={PhoneWL}",
                    creditScore, fraudRisk, isHighRiskId, emailWatchlist, phoneWatchlist);
            }
            catch (Exception ex)
            {
                // Resilient: log & continue with local-only score
                _logger.LogWarning(ex, "External checks failed for ProfileId={ProfileId}. Continuing with local-only score.", profile.Id);
            }
        }
        else if (request.UseExternalProviders)
        {
            _logger.LogWarning("UseExternalProviders requested, but external checks client is not configured. Skipping external calls.");
        }

        // ------------------------------------------
        // NEW: Blend external signals into final score
        // ------------------------------------------
        // Pull the score toward external credit score; penalize by fraud; apply watchlist penalties.
        if (creditScore is int cs)
        {
            score = (int)Math.Round(score * 0.6 + cs * 0.4, MidpointRounding.AwayFromZero);
        }

        if (fraudRisk is int fr)
        {
            score -= fr; // up to -100 points
        }

        if (isHighRiskId == true) score -= 80;
        if (emailWatchlist == true) score -= 30;
        if (phoneWatchlist == true) score -= 30;

        // Clamp and map decision
        score = Math.Clamp(score, 0, 1000);

        var decision = score switch
        {
            >= 700 => RiskDecision.Approve,
            >= 600 => RiskDecision.Review,
            _ => RiskDecision.Decline
        };

        // Recommendations (brief)
        var recs = BuildRecommendations(
            score,
            totalMonthlyIncome,
            yearsInCurrentJob,
            currentEmployment is not null,
            latestEarning is not null);

        _logger.LogInformation("Risk assessment completed with Score {Score}, Decision {Decision}", score, decision);

        // 4) Persist assessment
        var assessment = new RiskAssessment(profile.Id);
        assessment.RecordOutcome(score, decision, recs);

        _db.RiskAssessments.Add(assessment);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Risk assessment recorded with ID {RiskAssessmentId}", assessment.Id);

        return Map(assessment);
    }



    public async Task<RiskAssessmentReadDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving risk assessment with ID {RiskAssessmentId}", id);
        var e = await _db.RiskAssessments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null)
        {
            _logger.LogInformation("Risk assessment with ID {RiskAssessmentId} not found", id);
            return null;
        }
        else
        {
            return Map(e);
        }
    }

    public async Task<IReadOnlyList<RiskAssessmentReadDto>> ListByProfileAsync(Guid profileId, int skip = 0, int take = 50, CancellationToken ct = default)
    {
        _logger.LogInformation("Listing risk assessments for ProfileId {ProfileId}, skip {Skip}, take {Take}", profileId, skip, take);
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
