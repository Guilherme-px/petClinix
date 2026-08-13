namespace PetClinix.Modules.Pets.Application.Contracts;

public interface IPetsUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}