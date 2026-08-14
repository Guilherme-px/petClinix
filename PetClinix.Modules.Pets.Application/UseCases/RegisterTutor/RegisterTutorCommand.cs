using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Pets.Application.UseCases.RegisterTutor;

public sealed record RegisterTutorCommand(
    Guid ClinicId, Guid CreatedByUserId, string Name, string Cpf, string? Email, string PhoneNumber, string? SecondaryPhoneNumber,
    string ZipCode, string Street, string Number, string Neighborhood, string? Complement, string City, string State, string? Notes
) : ICommand<Result>;