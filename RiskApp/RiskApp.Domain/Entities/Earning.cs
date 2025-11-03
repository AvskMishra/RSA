using RiskApp.Domain.Abstraction;

namespace RiskApp.Domain.Entities;

public class Earning : BaseEntity
{
    public Guid ProfileId { get; private set; }
    public decimal MonthlyIncome { get; private set; }
    public decimal OtherMonthlyIncome { get; private set; }
    public string Currency { get; private set; } = "INR";
    public DateOnly EffectiveFrom { get; private set; }

    public Profile? Profile { get; private set; }

    private Earning() { }
    public Earning(Guid profileId, decimal monthlyIncome, decimal otherMonthlyIncome, DateOnly effectiveFrom, string currency = "INR")
    {
        ProfileId = profileId;
        MonthlyIncome = monthlyIncome;
        OtherMonthlyIncome = otherMonthlyIncome;
        EffectiveFrom = effectiveFrom;
        Currency = currency;
    }
}
