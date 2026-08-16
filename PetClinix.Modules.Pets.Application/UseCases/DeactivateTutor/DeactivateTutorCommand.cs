using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Pets.Application.UseCases.DeactivateTutor;

public sealed record DeactivateTutorCommand(Guid ClinicId, Guid TutorId) : ICommand<Result>;