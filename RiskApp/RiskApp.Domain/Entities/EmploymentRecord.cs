using RiskApp.Domain.Abstraction;
using RiskApp.Domain.Enums;

namespace RiskApp.Domain.Entities;

public class EmploymentRecord : BaseEntity
{
    public Guid ProfileId { get; private set; }
    public string EmployerName { get; private set; } = default!;
    public EmploymentType Type { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public bool IsCurrent { get; private set; }
    public decimal MonthlyIncome { get; private set; }

    // Nav back
    public Profile? Profile { get; private set; }

    private EmploymentRecord() { } // EF
    public EmploymentRecord(Guid profileId, string employerName, EmploymentType type, DateOnly start, decimal monthlyIncome, DateOnly? end = null)
    {
        ProfileId = profileId;
        EmployerName = employerName.Trim();
        Type = type;
        StartDate = start;
        EndDate = end;
        IsCurrent = end is null;
        MonthlyIncome = monthlyIncome;
    }

    public void Close(DateOnly endDate)
    {
        EndDate = endDate;
        IsCurrent = false;
        Touch();
    }
}
