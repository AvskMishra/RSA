using FluentValidation;
using RiskApp.Application.Profiles;

namespace RiskApp.Application.Validation;

public class ProfileCreateValidator : AbstractValidator<ProfileCreateDto>
{
    public ProfileCreateValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DateOfBirth).NotEmpty();
        RuleFor(x => x.NationalId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(30).When(x => !string.IsNullOrWhiteSpace(x.Phone));
        RuleFor(x => x.Address).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Address));
    }
}

public class ProfileUpdateValidator : AbstractValidator<ProfileUpdateDto>
{
    public ProfileUpdateValidator()
    {
        // All optional, but validate format/length when provided
        RuleFor(x => x.Email).EmailAddress().When(x => x.Email is not null && x.Email.Length > 0);
        RuleFor(x => x.Phone).MaximumLength(30).When(x => x.Phone is not null && x.Phone.Length > 0);
        RuleFor(x => x.Address).MaximumLength(500).When(x => x.Address is not null && x.Address.Length > 0);
    }
}
