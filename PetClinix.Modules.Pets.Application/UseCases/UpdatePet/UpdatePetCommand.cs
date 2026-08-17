using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Pets.Domain.Enums;

namespace PetClinix.Modules.Pets.Application.UseCases.UpdatePet;

public sealed record UpdatePetCommand(
    Guid ClinicId, Guid TutorId, Guid PetId, Guid UpdatedByUserId,
    string Name, Species Species, string? Breed, DateOnly? BirthDate,
    PetSex Sex, double? Weight, bool IsNeutered, string? Notes) : ICommand<Result>;