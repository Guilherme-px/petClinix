using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Domain.Enums;

namespace PetClinix.Modules.Identity.Application.UseCases.RegisterStaff;

public sealed record RegisterStaffCommand(
    Guid ClinicId,
    string Name,
    string Email,
    string DocumentNumber,
    string PhoneNumber,
    DateOnly BirthDate,
    UserRole Role) : ICommand<Result>;