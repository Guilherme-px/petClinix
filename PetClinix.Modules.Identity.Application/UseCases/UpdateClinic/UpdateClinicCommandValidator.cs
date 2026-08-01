using FluentValidation;

namespace PetClinix.Modules.Identity.Application.UseCases.UpdateClinic;

public sealed class UpdateClinicCommandValidator : AbstractValidator<UpdateClinicCommand>
{
    public UpdateClinicCommandValidator()
    {
        RuleFor(x => x.TradeName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.LegalName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DocumentNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Email).NotEmpty().MaximumLength(256);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ZipCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Street).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Number).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Neighborhood).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Complement).MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.Complement));
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.State).NotEmpty().MaximumLength(100);
    }
}