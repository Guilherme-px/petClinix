using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Pets.Application.Contracts;

namespace PetClinix.Modules.Pets.Infrastructure.Persistence;

public class UnitOfWork : IPetsUnitOfWork
{
    private readonly PetsDbContext _context;

    public UnitOfWork(PetsDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}