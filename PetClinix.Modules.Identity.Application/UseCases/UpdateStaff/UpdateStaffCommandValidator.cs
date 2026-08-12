using FluentValidation;
using PetClinix.Modules.Identity.Domain.Enums;

namespace PetClinix.Modules.Identity.Application.UseCases.UpdateStaff;

public sealed class UpdateStaffCommandValidator : AbstractValidator<UpdateStaffCommand>
{
    public UpdateStaffCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.BirthDate).NotEmpty().LessThan(DateOnly.FromDateTime(DateTime.UtcNow));
        RuleFor(x => x.Role).NotEqual(UserRole.Admin).WithMessage("Não é possível atribir a role de Admin por esta rota.");
    }
}