using PetClinix.Modules.Billing.Domain.Entities;

namespace PetClinix.Modules.Billing.Domain.Repositories;

public interface IWebhookEventRepository
{
    Task AddAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken = default);
    Task<bool> ExistsByStripeEventIdAsync(string stripeEventId, CancellationToken cancellationToken = default);
}