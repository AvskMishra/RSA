using MassTransit;
using RiskApp.Application.Messaging;
using RiskApp.Application.Risk.Providers;

namespace RiskApp.Infrastructure.Messaging;

public class ExternalChecksConsumer : IConsumer<PerformExternalChecks>
{
    private readonly ICreditProvider _credit;
    private readonly IFraudProvider _fraud;

    public ExternalChecksConsumer(ICreditProvider credit, IFraudProvider fraud)
    {
        _credit = credit;
        _fraud = fraud;
    }

    public async Task Consume(ConsumeContext<PerformExternalChecks> ctx)
    {
        var msg = ctx.Message;

        // Run providers (parallel)
        var creditTask = _credit.GetCreditScoreAsync(msg.NationalId, ctx.CancellationToken);
        var fraudTask = _fraud.GetSignalsAsync(msg.NationalId, msg.Email, msg.Phone, ctx.CancellationToken);
        await Task.WhenAll(creditTask, fraudTask);

        var credit = await creditTask;
        var fraud = await fraudTask;

        await ctx.RespondAsync<ExternalChecksCompleted>(new
        {
            msg.CorrelationId,
            msg.ProfileId,
            CreditScore = credit.Score,
            FraudRiskLevel = fraud.RiskLevel,
            fraud.IsHighRiskIdentity,
            fraud.EmailOnWatchlist,
            fraud.PhoneOnWatchlist,
            CompletedAtUtc = DateTime.UtcNow
        });
    }
}
