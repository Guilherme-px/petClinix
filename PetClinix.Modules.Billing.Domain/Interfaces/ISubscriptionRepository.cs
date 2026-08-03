using PetClinix.Modules.Billing.Domain.Entities;

namespace PetClinix.Modules.Billing.Domain.Interfaces;

public interface ISubscriptionRepository
{
    Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default);
    Task<Subscription?> GetByClinicIdAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);
}