using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Billing.Application.UseCases.ActivateSubscription;

public sealed record ActivateSubscriptionCommand(Guid ClinicId, string StripeCustomerId, string StripeSubscriptionId) : ICommand<Result>;