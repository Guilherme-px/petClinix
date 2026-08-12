using PetClinix.Modules.Identity.Domain.Enums;

namespace PetClinix.Modules.Identity.Application.UseCases.GetStaff;

public sealed record StaffResponse(
    Guid Id,
    string Name,
    string Email,
    string PhoneNumber,
    string DocumentNumber,
    DateOnly BirthDate,
    UserRole Role,
    bool IsActive);