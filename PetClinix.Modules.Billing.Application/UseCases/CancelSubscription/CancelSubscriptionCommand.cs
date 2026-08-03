using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Billing.Application.UseCases.CancelSubscription;

public sealed record CancelSubscriptionCommand(string StripeSubscriptionId) : ICommand<Result>;