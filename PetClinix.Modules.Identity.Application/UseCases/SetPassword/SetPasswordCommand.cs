using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Identity.Application.UseCases.SetPassword;

public sealed record SetPasswordCommand(string Token, string Password) : ICommand<Result>;