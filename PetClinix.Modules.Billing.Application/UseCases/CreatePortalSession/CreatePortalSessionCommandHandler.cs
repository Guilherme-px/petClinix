using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Billing.Application.Contracts;
using PetClinix.Modules.Billing.Domain.Interfaces;

namespace PetClinix.Modules.Billing.Application.UseCases.CreatePortalSession;

public sealed class CreatePortalSessionCommandHandler : ICommandHandler<CreatePortalSessionCommand, Result<CreatePortalSessionResponse>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IStripeService _stripeService;

    public CreatePortalSessionCommandHandler(ISubscriptionRepository subscriptionRepository, IStripeService stripeService)
    {
        _subscriptionRepository = subscriptionRepository;
        _stripeService = stripeService;
    }

    public async Task<Result<CreatePortalSessionResponse>> Handle(CreatePortalSessionCommand command, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByClinicIdAsync(command.ClinicId, cancellationToken);

        if (subscription == null)
        {
            return Result<CreatePortalSessionResponse>.Failure("billing.subscription.not_found", "Assinatura não encontrada para esta clínica.");
        }

        var portalUrl = await _stripeService.CreateBillingPortalSessionAsync(subscription.StripeCustomerId, command.ReturnUrl, cancellationToken);

        return Result<CreatePortalSessionResponse>.Success(new CreatePortalSessionResponse(portalUrl));
    }
}