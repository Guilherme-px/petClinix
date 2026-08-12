using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Identity.Application.UseCases.GetStaff;

public sealed record GetStaffQuery(Guid ClinicId) : ICommand<Result<List<StaffResponse>>>;