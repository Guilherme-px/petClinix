using Microsoft.EntityFrameworkCore;
using PetClinix.Modules.Billing.Domain.Entities;
using PetClinix.Modules.Billing.Domain.Interfaces;
using PetClinix.Modules.Billing.Infrastructure.Persistence;

namespace PetClinix.Modules.Billing.Infrastructure.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly BillingDbContext _context;

    public SubscriptionRepository(BillingDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        await _context.Subscriptions.AddAsync(subscription, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Subscription?> GetByClinicIdAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions.FirstOrDefaultAsync(s => s.ClinicId == clinicId, cancellationToken);
    }
}