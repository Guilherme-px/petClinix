using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Identity.Application.UseCases.GetProfile;

public sealed record GetProfileQuery(Guid UserId) : ICommand<Result<ProfileResponse>>;