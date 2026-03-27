namespace PetClinix.Modules.Identity.Application.UseCases.RegisterClinicWithAdmin;

public sealed class RegisterClinicWithAdminResponse
{
    public Guid ClinicId { get; init; }
    public Guid AdminUserId { get; init; }
}