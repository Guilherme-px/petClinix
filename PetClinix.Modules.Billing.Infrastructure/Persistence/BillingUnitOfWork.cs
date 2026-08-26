using PetClinix.Modules.Billing.Application.Contracts;

namespace PetClinix.Modules.Billing.Infrastructure.Persistence;

public class BillingUnitOfWork : IBillingUnitOfWork
{
    private readonly BillingDbContext _context;

    public BillingUnitOfWork(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}