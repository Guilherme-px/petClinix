namespace PetClinix.Modules.Billing.Application.Contracts;

public interface IBillingUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}