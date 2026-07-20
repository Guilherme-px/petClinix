using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Billing.Application.UseCases.CreateCheckoutSession;

public sealed record CreateCheckoutSessionCommand(Guid ClinicId, string PriceId, string SuccessUrl, string CancelUrl) : ICommand<Result<CreateCheckoutSessionResponse>>;