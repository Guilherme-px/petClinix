using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Identity.Application.UseCases.UpdateProfile;

public sealed record UpdateUserCommand(Guid UserId, string Name, string PhoneNumber, DateOnly BirthDate) : ICommand<Result>;