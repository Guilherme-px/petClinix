using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Billing.Application.UseCases.CreatePortalSession;

public sealed record CreatePortalSessionCommand(Guid ClinicId, string ReturnUrl) : ICommand<Result<CreatePortalSessionResponse>>;