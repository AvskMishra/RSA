using RiskApp.Domain.Abstraction;
using RiskApp.Domain.Enums;
namespace RiskApp.Domain.Entities;

public class RiskAssessment : BaseEntity
{
    public Guid ProfileId { get; private set; }
    public DateTime AssessedOnUtc { get; private set; } = DateTime.UtcNow;

    // Minimal fields now; we’ll enrich when we add rules/scorecards
    public int Score { get; private set; } = 0;
    public RiskDecision Decision { get; private set; } = RiskDecision.Pending;
    public string? Recommendations { get; private set; }
    public Profile? Profile { get; private set; }
    private RiskAssessment() { }
    public RiskAssessment(Guid profileId)
    {
        ProfileId = profileId;
    }
    public void RecordOutcome(int score, RiskDecision decision, string? recommendations = null)
    {
        Score = score;
        Decision = decision;
        Recommendations = recommendations;
        Touch();
    }
}
