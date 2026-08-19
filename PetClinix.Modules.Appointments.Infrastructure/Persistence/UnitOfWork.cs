using PetClinix.Modules.Appointments.Application.Contracts;

namespace PetClinix.Modules.Appointments.Infrastructure.Persistence;

public class UnitOfWork : IAppointmentsUnitOfWork
{
    private readonly AppointmentsDbContext _context;

    public UnitOfWork(AppointmentsDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}