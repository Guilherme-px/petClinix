using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Pets.Application.UseCases.UpdateTutor;

public sealed record UpdateTutorCommand(
    Guid ClinicId, Guid TutorId, string Name, string? Email, string PhoneNumber, string? SecondaryPhoneNumber,
    string ZipCode, string Street, string Number, string Neighborhood, string? Complement, string City, string State, string? Notes
) : ICommand<Result>;