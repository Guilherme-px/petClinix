namespace PetClinix.Modules.Identity.Application.Contracts;

public interface ISubscriptionStatusService
{
    Task<bool> IsClinicActiveAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<int> GetStaffLimitAsync(Guid clinicId, CancellationToken cancellationToken = default);
}