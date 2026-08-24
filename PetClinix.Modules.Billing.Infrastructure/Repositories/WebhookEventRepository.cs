using Microsoft.EntityFrameworkCore;
using PetClinix.Modules.Billing.Domain.Entities;
using PetClinix.Modules.Billing.Domain.Repositories;
using PetClinix.Modules.Billing.Infrastructure.Persistence;

namespace PetClinix.Modules.Billing.Infrastructure.Repositories;

public class WebhookEventRepository : IWebhookEventRepository
{
    private readonly BillingDbContext _context;

    public WebhookEventRepository(BillingDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        await _context.WebhookEvents.AddAsync(webhookEvent, cancellationToken);
    }

    public async Task<bool> ExistsByStripeEventIdAsync(string stripeEventId, CancellationToken cancellationToken = default)
    {
        return await _context.WebhookEvents.AnyAsync(w => w.StripeEventId == stripeEventId, cancellationToken);
    }
}