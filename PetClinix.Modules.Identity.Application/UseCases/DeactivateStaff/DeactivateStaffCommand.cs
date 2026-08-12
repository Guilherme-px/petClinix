using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Identity.Application.UseCases.DeactivateStaff;

public sealed record DeactivateStaffCommand(Guid ClinicId, Guid UserId) : ICommand<Result>;