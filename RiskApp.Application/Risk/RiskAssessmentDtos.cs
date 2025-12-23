using RiskApp.Domain.Enums;
namespace RiskApp.Application.Risk;
public record RiskAssessRequestDto
{
    public Guid ProfileId { get; init; }
    public bool UseExternalProviders { get; init; } = true;
}
public record RiskAssessmentReadDto
{
    public Guid Id { get; init; }
    public Guid ProfileId { get; init; }
    public DateTime AssessedOnUtc { get; init; }
    public int Score { get; init; }
    public RiskDecision Decision { get; init; }
    public string? Recommendations { get; init; }
}
