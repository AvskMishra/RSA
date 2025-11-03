using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RiskApp.Application.Analytics;
using RiskApp.Domain.Entities;
using RiskApp.Domain.Enums;
using RiskApp.Infrastructure.Persistence;

namespace RiskApp.Infrastructure.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly RiskAppDbContext _db;
    private readonly ILogger<AnalyticsService> _logger;
    public AnalyticsService(RiskAppDbContext db, ILogger<AnalyticsService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DecisionBucketDto>> GetDecisionDistributionAsync(Guid? profileId = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Calculating decision distribution{ProfileFilter}", profileId.HasValue ? $" for ProfileId {profileId}" : string.Empty);

        IQueryable<RiskAssessment> query = _db.RiskAssessments.AsNoTracking();

        if (query is null)
        {
            _logger.LogWarning("No RiskAssessments found in database.");
        }

        if (profileId.HasValue)
        {
            _logger.LogDebug("Applying profile filter for ProfileId {ProfileId}", profileId.Value);
            query = query!.Where(x => x.ProfileId == profileId.Value);
        }

        // Fetch grouped data as anonymous types with primitive properties
        var groupedData = await query!.GroupBy(x => x.Decision)
            .Select(g => new
            {
                Decision = g.Key,
                Count = g.Count()
            })
            .ToListAsync(ct);

        // Map to DTO and ensure all enum values appear even with zero count
        var result = Enum.GetValues<RiskDecision>()
            .Select(d =>
            {
                var item = groupedData.FirstOrDefault(x => x.Decision == d);
                return new DecisionBucketDto(d, item?.Count ?? 0);
            }).OrderByDescending(x => x.Count).ToList();

        return result;
    }


    public async Task<AverageScoreDto> GetAverageScoreAsync(Guid? profileId = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Calculating average score{ProfileFilter}", profileId.HasValue ? $" for ProfileId {profileId}" : string.Empty);
        var riskAssessmentData = _db.RiskAssessments.AsNoTracking();
        if (riskAssessmentData is null)
        {
            _logger.LogWarning("No RiskAssessments found in database.");
        }
        if (profileId.HasValue)
        {
            _logger.LogDebug("Applying profile filter for ProfileId {ProfileId}", profileId.Value);
            riskAssessmentData = riskAssessmentData!.Where(x => x.ProfileId == profileId.Value);
        }

        _logger.LogDebug("Calculating overall average score");
        var overall = await riskAssessmentData!.Select(x => (double?)x.Score).AverageAsync(ct) ?? 0d;

        _logger.LogDebug("Calculating average score per decision");
        var byDecision = await riskAssessmentData!.GroupBy(x => x.Decision)
                                .Select(g => new AverageScoreDto.DecisionAverage(g.Key, g.Average(x => x.Score)))
                                .ToListAsync(ct);

        // include zero buckets as 0
        _logger.LogDebug("Ensuring all decision buckets are represented");
        var all = Enum.GetValues<RiskDecision>()
            .Select(d => byDecision.FirstOrDefault(x => x.Decision == d) ?? new AverageScoreDto.DecisionAverage(d, 0))
            .ToList();

        return new AverageScoreDto(Math.Round(overall, 2), all.Select(x => x with { Average = Math.Round(x.Average, 2) }).ToList());
    }

    public async Task<IReadOnlyList<MonthlyTrendDto>> GetMonthlyTrendAsync(Guid? profileId = null, int monthsBack = 12, CancellationToken ct = default)
    {
        _logger.LogInformation("Calculating monthly trend for past {MonthsBack} months{ProfileFilter}", monthsBack, profileId.HasValue ? $" for ProfileId {profileId}" : string.Empty);
        monthsBack = Math.Clamp(monthsBack, 1, 60);

        var from = DateTime.UtcNow.Date.AddMonths(-monthsBack + 1); // include current month

        IQueryable<RiskAssessment> riskAssessmentData = _db.RiskAssessments.AsNoTracking().Where(x => x.AssessedOnUtc >= new DateTime(from.Year, from.Month, 1));

        if (profileId.HasValue)
        {
            _logger.LogDebug("Applying profile filter for ProfileId {ProfileId}", profileId.Value);
            riskAssessmentData = riskAssessmentData.Where(x => x.ProfileId == profileId.Value);
        }

        _logger.LogDebug("Fetching grouped monthly trend data");
        // Fetch grouping data as anonymous types
        var groupedData = await riskAssessmentData
            .GroupBy(x => new { x.AssessedOnUtc.Year, x.AssessedOnUtc.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count(),
                AverageScore = g.Average(x => x.Score)
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync(ct);

        _logger.LogDebug("Filling missing months in trend data");
        var filled = new List<MonthlyTrendDto>();
        for (int i = 0; i < monthsBack; i++)
        {
            var d = from.AddMonths(i);
            var hit = groupedData.FirstOrDefault(r => r.Year == d.Year && r.Month == d.Month);
            filled.Add(hit != null
                ? new MonthlyTrendDto(hit.Year, hit.Month, hit.Count, Math.Round(hit.AverageScore, 2))
                : new MonthlyTrendDto(d.Year, d.Month, 0, 0));
        }
        _logger.LogDebug("Completed monthly trend calculation");
        return filled;
    }

    public async Task<AnalyticsSummaryDto> GetSummaryAsync(Guid? profileId = null, int monthsBack = 12, CancellationToken ct = default)
    {
        // Run in parallel to minimize latency
        _logger.LogInformation("Starting parallel tasks with Task.WhenAll() for summary calculation");
        var decisionsTask = GetDecisionDistributionAsync(profileId, ct);
        var avgTask = GetAverageScoreAsync(profileId, ct);
        var trendTask = GetMonthlyTrendAsync(profileId, monthsBack, ct);

        await Task.WhenAll(decisionsTask, avgTask, trendTask);

        return new AnalyticsSummaryDto(
            Decisions: await decisionsTask,
            AverageScore: await avgTask,
            MonthlyTrend: await trendTask
        );
    }

}

