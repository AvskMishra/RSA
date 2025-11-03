using RiskApp.Domain.Enums;
using static RiskApp.Application.Analytics.AverageScoreDto;

namespace RiskApp.Application.Analytics;

public record AverageScoreDto(double Overall, IReadOnlyList<DecisionAverage> ByDecision)
{
    public record DecisionAverage(RiskDecision Decision, double Average);
}
