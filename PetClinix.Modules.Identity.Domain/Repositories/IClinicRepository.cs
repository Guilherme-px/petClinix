using PetClinix.Modules.Identity.Domain.Entities;
using PetClinix.Modules.Identity.Domain.ValueObjects;

namespace PetClinix.Modules.Identity.Domain.Repositories;

public interface IClinicRepository
{
    Task AddAsync(Clinic clinic, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySlugAsync(ClinicSlug slug, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<Clinic?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Clinic clinic, CancellationToken cancellationToken = default);
}