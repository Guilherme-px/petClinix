using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Catalog.Application.UseCases.DeactivateService;

public sealed record DeactivateServiceCommand(Guid ClinicId, Guid ServiceId) : ICommand<Result>;