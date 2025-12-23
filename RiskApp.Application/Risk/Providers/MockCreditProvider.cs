using RiskApp.Application.Risk.Providers;

namespace RiskApp.Infrastructure.Risk.Providers;

public class MockCreditProvider : ICreditProvider
{
    public Task<CreditScoreResult> GetCreditScoreAsync(string nationalId, CancellationToken ct = default)
    {
        // Deterministic pseudo-score from the ID (replace this with real API later)
        var baseNum = Math.Abs(nationalId.GetHashCode());
        var score = 300 + (baseNum % 601); // 300..901 -> clamp to 300..900
        score = Math.Min(score, 900);
        return Task.FromResult(new CreditScoreResult(score, "MockCredit", DateTime.UtcNow, "Deterministic mock score"));
    }
}
