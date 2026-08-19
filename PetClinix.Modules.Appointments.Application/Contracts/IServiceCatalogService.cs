namespace PetClinix.Modules.Appointments.Application.Contracts;

public interface IServiceCatalogService
{
    Task<int> GetDurationInMinutesAsync(Guid serviceId, CancellationToken cancellationToken = default);
}