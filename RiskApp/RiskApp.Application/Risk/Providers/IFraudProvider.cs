namespace RiskApp.Application.Risk.Providers;
public interface IFraudProvider
{
    /// <summary>Return fraud signals for a given national ID and optional email/phone.</summary>
    Task<FraudSignals> GetSignalsAsync(string nationalId, string? email, string? phone, CancellationToken ct = default);
}

public record FraudSignals(
    bool IsHighRiskIdentity,
    bool EmailOnWatchlist,
    bool PhoneOnWatchlist,
    int RiskLevel,// 0..100
    string Provider,
    DateTime RetrievedAtUtc,
    string? Notes = null
);
