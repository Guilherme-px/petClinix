namespace PetClinix.Modules.Identity.Application.UseCases.RegisterStaff;

public sealed record RegisterStaffResponse(Guid UserId, string? PasswordResetToken);