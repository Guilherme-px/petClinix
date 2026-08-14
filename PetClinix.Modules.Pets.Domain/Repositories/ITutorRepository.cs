using PetClinix.Modules.Pets.Domain.Entities;
using PetClinix.Modules.Pets.Domain.ValueObjects;

namespace PetClinix.Modules.Pets.Domain.Repositories;

public interface ITutorRepository
{
    Task AddAsync(Tutor tutor, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCpfAsync(Guid clinicId, Cpf cpf, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Tutor> Tutors, int TotalCount)> GetAllByClinicIdAsync(Guid clinicId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}