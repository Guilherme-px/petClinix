using PetClinix.Modules.Pets.Domain.Enums;

namespace PetClinix.Modules.Pets.Application.UseCases.GetPets;

public sealed record PetResponse(
    Guid Id,
    string Name,
    Species Species,
    string? Breed,
    DateOnly? BirthDate,
    PetSex Sex,
    double? Weight,
    bool IsNeutered
);