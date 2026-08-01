namespace PetClinix.Modules.Identity.Application.UseCases.GetProfile;

public sealed record ProfileResponse(
    Guid UserId,
    string Name,
    string Email,
    string DocumentNumber,
    string PhoneNumber,
    DateOnly BirthDate,
    string Role,
    ClinicResponse Clinic
);

public sealed record ClinicResponse(
    Guid ClinicId,
    string TradeName,
    string LegalName,
    string DocumentNumber,
    string Email,
    string PhoneNumber,
    string ZipCode,
    string Street,
    string Number,
    string Neighborhood,
    string? Complement,
    string City,
    string State
);