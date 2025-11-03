using FluentValidation;
using RiskApp.Application.Profiles;

public class ProfileUpdateValidator : AbstractValidator<ProfileUpdateDto>
{
    public ProfileUpdateValidator()
    {
        RuleFor(x => x.Email).EmailAddress().When(x => x.Email is not null && x.Email.Length > 0);
        RuleFor(x => x.Phone).MaximumLength(30).When(x => x.Phone is not null && x.Phone.Length > 0);
        RuleFor(x => x.Address).MaximumLength(500).When(x => x.Address is not null && x.Address.Length > 0);

        RuleFor(x => x).Must(HasAtLeastOneField)
            .WithMessage("Provide at least one field to update (Email, Phone, or Address).");
    }

    private static bool HasAtLeastOneField(ProfileUpdateDto dto) =>
        !(string.IsNullOrWhiteSpace(dto.Email) && string.IsNullOrWhiteSpace(dto.Phone) && string.IsNullOrWhiteSpace(dto.Address));
}
