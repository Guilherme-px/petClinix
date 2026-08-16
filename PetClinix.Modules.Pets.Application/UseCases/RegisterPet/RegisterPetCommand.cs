using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Pets.Domain.Enums;

namespace PetClinix.Modules.Pets.Application.UseCases.RegisterPet;

public sealed record RegisterPetCommand(
    Guid ClinicId,
    Guid TutorId,
    Guid CreatedByUserId,
    string Name,
    Species Species,
    string? Breed,
    DateOnly? BirthDate,
    PetSex Sex,
    double? Weight,
    bool IsNeutered,
    string? Notes) : ICommand<Result>;