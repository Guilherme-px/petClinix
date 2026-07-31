using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Identity.Application.UseCases.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<Result<RefreshTokenResponse>>;