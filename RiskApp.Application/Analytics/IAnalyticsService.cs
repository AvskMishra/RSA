
namespace RiskApp.Application.Analytics;

public interface IAnalyticsService
{
    Task<AverageScoreDto> GetAverageScoreAsync(Guid? profileId = null, CancellationToken ct = default);
    Task<IReadOnlyList<DecisionBucketDto>> GetDecisionDistributionAsync(Guid? profileId = null, CancellationToken ct = default);
    Task<IReadOnlyList<MonthlyTrendDto>> GetMonthlyTrendAsync(Guid? profileId = null, int monthsBack = 12, CancellationToken ct = default);
    Task<AnalyticsSummaryDto> GetSummaryAsync(Guid? profileId = null, int monthsBack = 12, CancellationToken ct = default);
}