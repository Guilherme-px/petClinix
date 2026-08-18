using FluentValidation;

namespace PetClinix.Modules.Catalog.Application.UseCases.RegisterService;

public sealed class RegisterServiceCommandValidator : AbstractValidator<RegisterServiceCommand>
{
    public RegisterServiceCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Description));
        RuleFor(x => x.DurationInMinutes).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}