using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Billing.Domain.Enums;

namespace PetClinix.Modules.Billing.Application.UseCases.ActivateSubscription;

public sealed record ActivateSubscriptionCommand(Guid ClinicId, string StripeCustomerId, string StripeSubscriptionId, PlanTier PlanTier) : ICommand<Result>;