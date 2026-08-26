using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Billing.Application.Contracts;
using PetClinix.Modules.Billing.Domain.Entities;
using PetClinix.Modules.Billing.Domain.Repositories;

namespace PetClinix.Modules.Billing.Application.UseCases.ActivateSubscription;

public sealed class ActivateSubscriptionCommandHandler : ICommandHandler<ActivateSubscriptionCommand, Result>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IBillingUnitOfWork _unitOfWork;

    public ActivateSubscriptionCommandHandler(
        ISubscriptionRepository subscriptionRepository,
        IBillingUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ActivateSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var existingSubscription = await _subscriptionRepository.GetByClinicIdAsync(command.ClinicId, cancellationToken);

        if (existingSubscription != null)
        {
            return Result.Success();
        }

        var subscription = Subscription.Create(command.ClinicId, command.StripeCustomerId, command.StripeSubscriptionId, command.PlanTier);

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}