using RiskApp.Domain.Enums;

namespace RiskApp.Application.Work;

public record EmploymentCreateDto
{
    public Guid ProfileId { get; init; }
    public string EmployerName { get; init; } = default!;
    public EmploymentType Type { get; init; }
    public DateOnly StartDate { get; init; }
    public decimal MonthlyIncome { get; init; }
    public DateOnly? EndDate { get; init; }
}
public record EmploymentReadDto
{
    public Guid Id { get; init; }
    public Guid ProfileId { get; init; }
    public string EmployerName { get; init; } = default!;
    public EmploymentType Type { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public bool IsCurrent { get; init; }
    public decimal MonthlyIncome { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}

public record EmploymentUpdateDto
{
    public decimal MonthlyIncome { get; init; }
    public EmploymentType Type { get; init; }
    public string? EmployerName { get; init; }
}

public record EmploymentCloseDto
{
    public DateOnly EndDate { get; init; }
}
