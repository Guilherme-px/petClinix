using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Identity.Application.UseCases.GetStaff;

public sealed record GetStaffByIdQuery(Guid ClinicId, Guid UserId) : ICommand<Result<StaffResponse>>;