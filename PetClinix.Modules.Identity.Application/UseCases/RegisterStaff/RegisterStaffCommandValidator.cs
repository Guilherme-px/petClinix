using FluentValidation;
using PetClinix.Modules.Identity.Domain.Enums;

namespace PetClinix.Modules.Identity.Application.UseCases.RegisterStaff;

public sealed class RegisterStaffCommandValidator : AbstractValidator<RegisterStaffCommand>
{
    public RegisterStaffCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().MaximumLength(256);
        RuleFor(x => x.DocumentNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.BirthDate).NotEmpty().LessThan(DateOnly.FromDateTime(DateTime.UtcNow));
        RuleFor(x => x.Role).NotEqual(UserRole.Admin).WithMessage("Não é possível cadastrar um usuário Admin por esta rota.");
    }
}