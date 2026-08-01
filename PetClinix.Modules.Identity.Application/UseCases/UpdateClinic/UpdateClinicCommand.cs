using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Identity.Application.UseCases.UpdateClinic;

public sealed record UpdateClinicCommand(
    Guid ClinicId,
    string TradeName, string LegalName, string DocumentNumber,
    string Email, string PhoneNumber,
    string ZipCode, string Street, string Number, string Neighborhood,
    string? Complement, string City, string State) : ICommand<Result>;