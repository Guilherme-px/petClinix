using PetClinix.Modules.Catalog.Application.Contracts;

namespace PetClinix.Modules.Catalog.Infrastructure.Persistence;

public class UnitOfWork : ICatalogUnitOfWork
{
    private readonly CatalogDbContext _context;

    public UnitOfWork(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}