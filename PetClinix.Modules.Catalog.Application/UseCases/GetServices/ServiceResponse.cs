namespace PetClinix.Modules.Catalog.Application.UseCases.GetServices;

public sealed record ServiceResponse(
    Guid Id,
    string Name,
    string? Description,
    int DurationInMinutes,
    decimal Price,
    bool RequiresVeterinarian
);