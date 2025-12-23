using RiskApp.Application.Risk.Providers;
using System;

namespace RiskApp.Infrastructure.Risk.Providers;

public class MockFraudProvider : IFraudProvider
{
    public Task<FraudSignals> GetSignalsAsync(string nationalId, string? email, string? phone, CancellationToken ct = default)
    {
        // Simple heuristics: if ID ends with 'X' => a bit risky; if email contains 'test' => watchlist
        bool idHighRisk = nationalId.EndsWith("X", StringComparison.OrdinalIgnoreCase);
        bool emailWatch = (email ?? string.Empty).Contains("test", StringComparison.OrdinalIgnoreCase);
        bool phoneWatch = (phone ?? string.Empty).EndsWith("0000");

        var risk = 0;
        if (idHighRisk) risk += 40;
        if (emailWatch) risk += 30;
        if (phoneWatch) risk += 30;
        risk = Math.Clamp(risk, 0, 100);

        return Task.FromResult(new FraudSignals(
            IsHighRiskIdentity: idHighRisk,
            EmailOnWatchlist: emailWatch,
            PhoneOnWatchlist: phoneWatch,
            RiskLevel: risk,
            Provider: "MockFraud",
            RetrievedAtUtc: DateTime.UtcNow,
            Notes: "Heuristic mock signals"
        ));
    }
}
