using FluentValidation;
using RiskApp.Application.Risk;

namespace RiskApp.Application.Validation;

public class RiskAssessRequestValidator : AbstractValidator<RiskAssessRequestDto>
{
    public RiskAssessRequestValidator()
    {
        RuleFor(x => x.ProfileId).NotEmpty();
        // UseExternalProviders is a bool; no rule needed
    }
}
