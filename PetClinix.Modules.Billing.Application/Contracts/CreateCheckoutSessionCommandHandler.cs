using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Billing.Application.Contracts;

namespace PetClinix.Modules.Billing.Application.UseCases.CreateCheckoutSession;

public sealed class CreateCheckoutSessionCommandHandler : ICommandHandler<CreateCheckoutSessionCommand, Result<CreateCheckoutSessionResponse>>
{
    private readonly IStripeService _stripeService;

    public CreateCheckoutSessionCommandHandler(IStripeService stripeService)
    {
        _stripeService = stripeService;
    }

    public async Task<Result<CreateCheckoutSessionResponse>> Handle(CreateCheckoutSessionCommand command, CancellationToken cancellationToken)
    {
        var checkoutUrl = await _stripeService.CreateCheckoutSessionAsync(command, cancellationToken);

        return Result<CreateCheckoutSessionResponse>.Success(new CreateCheckoutSessionResponse(checkoutUrl));
    }
}