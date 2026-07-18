using Microsoft.EntityFrameworkCore;
using PetClinix.Modules.Identity.Domain.Entities;
using PetClinix.Modules.Identity.Domain.Repositories;
using PetClinix.Modules.Identity.Domain.ValueObjects;
using PetClinix.Modules.Identity.Infrastructure.Persistence;

namespace PetClinix.Modules.Identity.Infrastructure.Repositories;

public class ClinicRepository : IClinicRepository
{
    private readonly IdentityDbContext _context;

    public ClinicRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Clinic clinic, CancellationToken cancellationToken = default)
    {
        await _context.Clinics.AddAsync(clinic, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsBySlugAsync(ClinicSlug slug, CancellationToken cancellationToken = default)
    {
        return await _context.Clinics.AnyAsync(c => c.Slug == slug, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _context.Clinics.AnyAsync(c => c.Email == email, cancellationToken);
    }
}