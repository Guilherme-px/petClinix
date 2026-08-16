using Microsoft.EntityFrameworkCore;
using PetClinix.Modules.Pets.Domain.Entities;
using PetClinix.Modules.Pets.Domain.Repositories;
using PetClinix.Modules.Pets.Infrastructure.Persistence;

namespace PetClinix.Modules.Pets.Infrastructure.Repositories;

public class PetRepository : IPetRepository
{
    private readonly PetsDbContext _context;

    public PetRepository(PetsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Pet pet, CancellationToken cancellationToken = default)
    {
        await _context.Pets.AddAsync(pet, cancellationToken);
    }

    public async Task<bool> ExistsByNameAndTutorAsync(Guid tutorId, string name, CancellationToken cancellationToken = default)
    {
        return await _context.Pets.AnyAsync(p => p.TutorId == tutorId && p.Name == name, cancellationToken);
    }

    public async Task<Pet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Pets.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(Pet pet, CancellationToken cancellationToken = default)
    {
        _context.Pets.Update(pet);
    }
}