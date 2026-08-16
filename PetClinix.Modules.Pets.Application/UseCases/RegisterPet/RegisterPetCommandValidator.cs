using FluentValidation;

namespace PetClinix.Modules.Pets.Application.UseCases.RegisterPet;

public sealed class RegisterPetCommandValidator : AbstractValidator<RegisterPetCommand>
{
    public RegisterPetCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Breed).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Breed));
        RuleFor(x => x.Notes).MaximumLength(1000).When(x => !string.IsNullOrWhiteSpace(x.Notes));
        RuleFor(x => x.Weight).GreaterThan(0).When(x => x.Weight.HasValue);
    }
}