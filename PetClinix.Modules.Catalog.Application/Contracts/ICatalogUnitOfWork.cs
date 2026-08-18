namespace PetClinix.Modules.Catalog.Application.Contracts;

public interface ICatalogUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}