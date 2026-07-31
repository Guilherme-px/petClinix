using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Identity.Application.UseCases.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<Result<LoginResponse>>;