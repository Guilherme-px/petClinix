using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Billing.Domain.Entities;
using PetClinix.Modules.Billing.Domain.Interfaces;

namespace PetClinix.Modules.Billing.Application.UseCases.ActivateSubscription;

public sealed class ActivateSubscriptionCommandHandler : ICommandHandler<ActivateSubscriptionCommand, Result>
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public ActivateSubscriptionCommandHandler(ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<Result> Handle(ActivateSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var existingSubscription = await _subscriptionRepository.GetByClinicIdAsync(command.ClinicId, cancellationToken);

        if (existingSubscription != null)
        {
            return Result.Success();
        }

        var subscription = Subscription.Create(
            command.ClinicId,
            command.StripeCustomerId,
            command.StripeSubscriptionId);

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);

        return Result.Success();
    }
}