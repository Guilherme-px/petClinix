namespace PetClinix.Modules.Pets.Application.UseCases.GetTutors;

public sealed record TutorResponse(
    Guid Id,
    string Name,
    string Cpf,
    string? Email,
    string PhoneNumber,
    string? SecondaryPhoneNumber,
    string City,
    string State,
    bool IsActive
);