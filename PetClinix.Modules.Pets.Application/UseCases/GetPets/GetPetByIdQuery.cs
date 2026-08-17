using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Pets.Application.UseCases.GetPets;

public sealed record GetPetByIdQuery(Guid ClinicId, Guid TutorId, Guid PetId) : ICommand<Result<PetResponse>>;