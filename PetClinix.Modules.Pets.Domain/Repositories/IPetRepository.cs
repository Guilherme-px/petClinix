using PetClinix.Modules.Pets.Domain.Entities;

namespace PetClinix.Modules.Pets.Domain.Repositories;

public interface IPetRepository
{
    Task AddAsync(Pet pet, CancellationToken cancellationToken = default);
    Task<Pet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAndTutorAsync(Guid tutorId, string name, CancellationToken cancellationToken = default);
    Task UpdateAsync(Pet pet, CancellationToken cancellationToken = default);
}