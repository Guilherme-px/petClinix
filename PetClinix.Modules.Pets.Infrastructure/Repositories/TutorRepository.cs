using Microsoft.EntityFrameworkCore;
using PetClinix.Modules.Pets.Domain.Entities;
using PetClinix.Modules.Pets.Domain.Repositories;
using PetClinix.Modules.Pets.Domain.ValueObjects;
using PetClinix.Modules.Pets.Infrastructure.Persistence;

namespace PetClinix.Modules.Pets.Infrastructure.Repositories;

public class TutorRepository : ITutorRepository
{
    private readonly PetsDbContext _context;

    public TutorRepository(PetsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Tutor tutor, CancellationToken cancellationToken = default)
    {
        await _context.Tutors.AddAsync(tutor, cancellationToken);
    }

    public async Task <bool> ExistsByCpfAsync(Guid clinicId, Cpf cpf, CancellationToken cancellationToken = default)
    {
        return await _context.Tutors.AnyAsync(t => t.ClinicId == clinicId && t.Cpf == cpf, cancellationToken);
    }
}