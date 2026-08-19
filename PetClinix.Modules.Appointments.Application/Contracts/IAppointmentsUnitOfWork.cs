namespace PetClinix.Modules.Appointments.Application.Contracts;

public interface IAppointmentsUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}