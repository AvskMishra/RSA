namespace RiskApp.Application.Analytics;
public record AnalyticsSummaryDto(
    IReadOnlyList<DecisionBucketDto> Decisions,
    AverageScoreDto AverageScore,
    IReadOnlyList<MonthlyTrendDto> MonthlyTrend
);
