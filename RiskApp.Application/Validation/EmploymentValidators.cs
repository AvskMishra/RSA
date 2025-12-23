using FluentValidation;
using RiskApp.Application.Work;
using RiskApp.Domain.Enums;

namespace RiskApp.Application.Validation;

public class EmploymentCreateValidator : AbstractValidator<EmploymentCreateDto>
{
    public EmploymentCreateValidator()
    {
        RuleFor(x => x.ProfileId).NotEmpty();
        RuleFor(x => x.EmployerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.MonthlyIncome).GreaterThanOrEqualTo(0);
        // If EndDate provided, it should be >= StartDate
        RuleFor(x => x.EndDate)
            .Must((dto, end) => end is null || end.Value >= dto.StartDate)
            .WithMessage("EndDate must be on or after StartDate.");
    }
}

public class EmploymentUpdateValidator : AbstractValidator<EmploymentUpdateDto>
{
    public EmploymentUpdateValidator()
    {
        RuleFor(x => x.MonthlyIncome).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.EmployerName).MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.EmployerName));
    }
}

public class EmploymentCloseValidator : AbstractValidator<EmploymentCloseDto>
{
    public EmploymentCloseValidator()
    {
        RuleFor(x => x.EndDate).NotEmpty();
    }
}
