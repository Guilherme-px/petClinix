using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Pets.Application.UseCases.DeactivatePet;

public sealed record DeactivatePetCommand(Guid ClinicId, Guid TutorId, Guid PetId) : ICommand<Result>;