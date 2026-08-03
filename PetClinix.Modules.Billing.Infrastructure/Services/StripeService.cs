using Microsoft.Extensions.Configuration;
using PetClinix.Modules.Billing.Application.Contracts;
using PetClinix.Modules.Billing.Application.UseCases.CreateCheckoutSession;
using Stripe;
using Stripe.Checkout;

namespace PetClinix.Modules.Billing.Infrastructure.Services;

public class StripeService : IStripeService
{
    private readonly SessionService _sessionService;

    public StripeService(IConfiguration configuration)
    {
        var secretKey = configuration["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe SecretKey não configurada.");

        StripeConfiguration.ApiKey = secretKey;
        _sessionService = new SessionService();
    }

    public async Task<string> CreateCheckoutSessionAsync(CreateCheckoutSessionCommand command, CancellationToken cancellationToken)
    {
        var options = new SessionCreateOptions
        {
            Mode = "subscription",
            ClientReferenceId = command.ClinicId.ToString(),
            CustomerEmail = command.AdminEmail,
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = command.PriceId,
                    Quantity = 1
                }
            },
            SuccessUrl = command.SuccessUrl,
            CancelUrl = command.CancelUrl
        };

        var session = await _sessionService.CreateAsync(options, cancellationToken: cancellationToken);

        return session.Url;
    }

    public async Task<string> CreateBillingPortalSessionAsync(string stripeCustomerId, string returnUrl, CancellationToken cancellationToken)
    {
        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = stripeCustomerId,
            ReturnUrl = returnUrl
        };

        var service = new Stripe.BillingPortal.SessionService();
        var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

        return session.Url;
    }   
}