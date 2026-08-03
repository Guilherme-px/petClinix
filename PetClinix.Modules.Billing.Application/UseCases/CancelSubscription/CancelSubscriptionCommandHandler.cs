using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Billing.Domain.Interfaces;

namespace PetClinix.Modules.Billing.Application.UseCases.CancelSubscription;

public sealed class CancelSubscriptionCommandHandler : ICommandHandler<CancelSubscriptionCommand, Result>
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public CancelSubscriptionCommandHandler(ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<Result> Handle(CancelSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(command.StripeSubscriptionId, cancellationToken);

        if (subscription == null)
        {
            return Result.Failure("billing.subscription.not_found", "Assinatura não encontrada.");
        }

        subscription.MarkAsCanceled();
        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);

        return Result.Success();
    }
}