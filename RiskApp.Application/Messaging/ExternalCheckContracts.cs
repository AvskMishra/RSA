namespace RiskApp.Application.Messaging;

//putting properties in interfaces to allow for easier mocking in tests
//allowed in latest versions of C#
public interface PerformExternalChecks
{
    Guid CorrelationId { get; }
    Guid ProfileId { get; }
    string NationalId { get; }
    string? Email { get; }
    string? Phone { get; }
}

public interface ExternalChecksCompleted
{
    Guid CorrelationId { get; }
    Guid ProfileId { get; }
    int CreditScore { get; }    // between range 300..900
    int FraudRiskLevel { get; } // between range 0..100
    bool IsHighRiskIdentity { get; }
    bool EmailOnWatchlist { get; }
    bool PhoneOnWatchlist { get; }
    DateTime CompletedAtUtc { get; }
}
