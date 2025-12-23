using RiskApp.Domain.Enums;

namespace RiskApp.Application.Analytics;

public record DecisionBucketDto(RiskDecision Decision, int Count);
