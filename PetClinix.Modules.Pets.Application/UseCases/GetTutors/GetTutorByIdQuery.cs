using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Pets.Application.UseCases.GetTutors;

public sealed record GetTutorByIdQuery(Guid ClinicId, Guid TutorId) : ICommand<Result<TutorResponse>>;