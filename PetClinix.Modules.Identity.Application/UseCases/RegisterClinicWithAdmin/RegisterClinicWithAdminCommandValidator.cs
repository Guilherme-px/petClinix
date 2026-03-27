using FluentValidation;

namespace PetClinix.Modules.Identity.Application.UseCases.RegisterClinicWithAdmin;

public sealed class RegisterClinicWithAdminCommandValidator : AbstractValidator<RegisterClinicWithAdminCommand>
{
    public RegisterClinicWithAdminCommandValidator()
    {
        RuleFor(x => x.TradeName)
            .NotEmpty().WithMessage("O nome fantasia da clínica é obrigatório.")
            .MaximumLength(150).WithMessage("O nome fantasia da clínica deve ter no máximo 150 caracteres.");

        RuleFor(x => x.LegalName)
            .MaximumLength(150).WithMessage("A razão social da clínica deve ter no máximo 150 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.LegalName));

        RuleFor(x => x.DocumentNumber)
            .MaximumLength(30).WithMessage("O documento da clínica deve ter no máximo 30 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.DocumentNumber));

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("O identificador da clínica é obrigatório.")
            .MinimumLength(3).WithMessage("O identificador da clínica deve ter no mínimo 3 caracteres.")
            .MaximumLength(50).WithMessage("O identificador da clínica deve ter no máximo 50 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail da clínica é obrigatório.")
            .MaximumLength(256).WithMessage("O e-mail da clínica deve ter no máximo 256 caracteres.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("O telefone da clínica é obrigatório.")
            .MaximumLength(20).WithMessage("O telefone da clínica deve ter no máximo 20 caracteres.");

        RuleFor(x => x.AddressLine)
            .MaximumLength(200).WithMessage("O endereço da clínica deve ter no máximo 200 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.AddressLine));

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("A cidade deve ter no máximo 100 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.City));

        RuleFor(x => x.State)
            .MaximumLength(100).WithMessage("O estado deve ter no máximo 100 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.State));

        RuleFor(x => x.ZipCode)
            .MaximumLength(20).WithMessage("O CEP deve ter no máximo 20 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.ZipCode));

        RuleFor(x => x.AdminName)
            .NotEmpty().WithMessage("O nome do administrador é obrigatório.")
            .MaximumLength(150).WithMessage("O nome do administrador deve ter no máximo 150 caracteres.");

        RuleFor(x => x.AdminEmail)
            .NotEmpty().WithMessage("O e-mail do administrador é obrigatório.")
            .MaximumLength(256).WithMessage("O e-mail do administrador deve ter no máximo 256 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(8).WithMessage("A senha deve ter no mínimo 8 caracteres.")
            .MaximumLength(100).WithMessage("A senha deve ter no máximo 100 caracteres.");
    }
}