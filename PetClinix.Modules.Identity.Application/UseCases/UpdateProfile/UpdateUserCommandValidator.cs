using FluentValidation;

namespace PetClinix.Modules.Identity.Application.UseCases.UpdateProfile;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(150).WithMessage("O nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("O telefone é obrigatório.");

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("A data de nascimento é obrigatória.")
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Data de nascimento inválida.");
    }
}