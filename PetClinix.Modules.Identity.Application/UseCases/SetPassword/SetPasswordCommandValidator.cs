using FluentValidation;

namespace PetClinix.Modules.Identity.Application.UseCases.SetPassword;

public sealed class SetPasswordCommandValidator : AbstractValidator<SetPasswordCommand>
{
    public SetPasswordCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("O token é obrigatório.");

        RuleFor(x => x.Password)
           .NotEmpty().WithMessage("A senha é obrigatória.")
           .MinimumLength(8).WithMessage("A senha deve ter no mínimo 8 caracteres.")
           .MaximumLength(100).WithMessage("A senha deve ter no máximo 100 caracteres.")
           .Matches("[A-Z]").WithMessage("A senha deve conter pelo menos uma letra maiúscula.")
           .Matches("[^a-zA-Z0-9]").WithMessage("A senha deve conter pelo menos um caractere especial.");
    }
}