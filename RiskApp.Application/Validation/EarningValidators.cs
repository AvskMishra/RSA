using FluentValidation;
using RiskApp.Application.Earnings;

namespace RiskApp.Application.Validation;

public class EarningCreateValidator : AbstractValidator<EarningCreateDto>
{
    public EarningCreateValidator()
    {
        RuleFor(x => x.ProfileId).NotEmpty();
        RuleFor(x => x.MonthlyIncome).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OtherMonthlyIncome).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(10);
        RuleFor(x => x.EffectiveFrom).NotEmpty();
    }
}

public class EarningUpdateValidator : AbstractValidator<EarningUpdateDto>
{
    public EarningUpdateValidator()
    {
        RuleFor(x => x.MonthlyIncome).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OtherMonthlyIncome).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).MaximumLength(10).When(x => !string.IsNullOrWhiteSpace(x.Currency));
        // EffectiveFrom optional; no cross-validation here
    }
}
