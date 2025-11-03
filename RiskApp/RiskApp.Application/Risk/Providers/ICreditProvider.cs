namespace RiskApp.Application.Risk.Providers;
public interface ICreditProvider
{
    /// <summary>Fetch a credit score for a national ID; range typically 300-900.</summary>
    Task<CreditScoreResult> GetCreditScoreAsync(string nationalId, CancellationToken ct = default);
}
public record CreditScoreResult(int Score, string Provider, DateTime RetrievedAtUtc, string? Notes = null);
