namespace RiskApp.Application.Earnings;

public record EarningCreateDto
{
    public Guid ProfileId { get; init; }
    public decimal MonthlyIncome { get; init; }
    public decimal OtherMonthlyIncome { get; init; }
    public string Currency { get; init; } = "INR";
    public DateOnly EffectiveFrom { get; init; }
}
public record EarningReadDto
{
    public Guid Id { get; init; }
    public Guid ProfileId { get; init; }
    public decimal MonthlyIncome { get; init; }
    public decimal OtherMonthlyIncome { get; init; }
    public string Currency { get; init; } = "INR";
    public DateOnly EffectiveFrom { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}
public record EarningUpdateDto
{
    public decimal MonthlyIncome { get; init; }
    public decimal OtherMonthlyIncome { get; init; }
    public string? Currency { get; init; }
    public DateOnly? EffectiveFrom { get; init; }
}
