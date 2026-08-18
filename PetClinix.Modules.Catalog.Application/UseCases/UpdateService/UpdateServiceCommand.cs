using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Catalog.Application.UseCases.UpdateService;

public sealed record UpdateServiceCommand(
    Guid ClinicId, Guid ServiceId, Guid UpdatedByUserId,
    string Name, string? Description, int DurationInMinutes, decimal Price, bool RequiresVeterinarian) : ICommand<Result>;