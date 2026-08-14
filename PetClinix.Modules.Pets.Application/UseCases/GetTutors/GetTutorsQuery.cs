using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Pets.Application.UseCases.GetTutors;

public sealed record GetTutorsQuery(Guid ClinicId, int PageNumber, int PageSize) : ICommand<Result<PagedResult<TutorResponse>>>;