using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Catalog.Application.UseCases.RegisterService;

public sealed record RegisterServiceCommand(
    Guid ClinicId,
    Guid CreatedByUserId,
    string Name,
    string? Description,
    int DurationInMinutes,
    decimal Price,
    bool RequiresVeterinarian
) : ICommand<Result>;