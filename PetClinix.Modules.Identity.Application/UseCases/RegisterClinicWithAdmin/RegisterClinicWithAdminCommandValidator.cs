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
            .NotEmpty().WithMessage("A razão social da clínica é obrigatória.")
            .MaximumLength(150).WithMessage("A razão social da clínica deve ter no máximo 150 caracteres.");

        RuleFor(x => x.DocumentNumber) 
            .NotEmpty().WithMessage("O CNPJ da clínica é obrigatório.")
            .MaximumLength(30).WithMessage("O CNPJ da clínica deve ter no máximo 30 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail da clínica é obrigatório.")
            .MaximumLength(256).WithMessage("O e-mail da clínica deve ter no máximo 256 caracteres.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("O telefone da clínica é obrigatório.")
            .MaximumLength(20).WithMessage("O telefone da clínica deve ter no máximo 20 caracteres.");

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("O CEP da clínica é obrigatório.")
            .MaximumLength(20).WithMessage("O CEP deve ter no máximo 20 caracteres.");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("A rua da clínica é obrigatória.")
            .MaximumLength(200).WithMessage("A rua deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("O número da clínica é obrigatório.")
            .MaximumLength(20).WithMessage("O número deve ter no máximo 20 caracteres.");

        RuleFor(x => x.Neighborhood)
            .NotEmpty().WithMessage("O bairro da clínica é obrigatório.")
            .MaximumLength(100).WithMessage("O bairro deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Complement)
            .MaximumLength(200).WithMessage("O complemento deve ter no máximo 200 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Complement));

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("A cidade da clínica é obrigatória.")
            .MaximumLength(100).WithMessage("A cidade deve ter no máximo 100 caracteres.");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("O estado da clínica é obrigatório.")
            .MaximumLength(100).WithMessage("O estado deve ter no máximo 100 caracteres.");

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

        RuleFor(x => x.AdminDocumentNumber)
            .NotEmpty().WithMessage("O CPF do administrador é obrigatório.")
            .MaximumLength(20).WithMessage("O CPF deve ter no máximo 20 caracteres.");

        RuleFor(x => x.AdminPhoneNumber)
            .NotEmpty().WithMessage("O telefone do administrador é obrigatório.")
            .MaximumLength(20).WithMessage("O telefone do administrador deve ter no máximo 20 caracteres.");

        RuleFor(x => x.AdminBirthDate)
            .NotEmpty().WithMessage("A data de nascimento do administrador é obrigatória.")
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("A data de nascimento deve ser uma data válida no passado.");
    }
}