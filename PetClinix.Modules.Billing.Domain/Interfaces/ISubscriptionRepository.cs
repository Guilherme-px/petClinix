using PetClinix.Modules.Billing.Domain.Entities;

namespace PetClinix.Modules.Billing.Domain.Interfaces;

public interface ISubscriptionRepository
{
    Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default);
    Task<Subscription?> GetByClinicIdAsync(Guid clinicId, CancellationToken cancellationToken = default);
}