using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Domain.Enums;

namespace PetClinix.Modules.Identity.Application.UseCases.UpdateStaff;

public sealed record UpdateStaffCommand(
    Guid ClinicId, Guid UserId, Guid UpdatedByUserId, string Name, string PhoneNumber,
    DateOnly BirthDate, UserRole Role
) : ICommand<Result>;