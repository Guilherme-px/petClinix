using PetClinix.Modules.Billing.Application.UseCases.CreateCheckoutSession;

namespace PetClinix.Modules.Billing.Application.Contracts;

public interface IStripeService
{
    Task<string> CreateCheckoutSessionAsync(CreateCheckoutSessionCommand command, CancellationToken cancellationToken);
}