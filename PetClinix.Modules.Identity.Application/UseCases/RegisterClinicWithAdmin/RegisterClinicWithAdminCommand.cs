using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Identity.Application.UseCases.RegisterClinicWithAdmin;

public sealed class RegisterClinicWithAdminCommand : ICommand<Result<RegisterClinicWithAdminResponse>>
{
    public string TradeName { get; init; } = string.Empty;
    public string LegalName { get; init; } = string.Empty;
    public string DocumentNumber { get; init; } = string.Empty; 
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    
    public string ZipCode { get; init; } = string.Empty;
    public string Street { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public string Neighborhood { get; init; } = string.Empty;
    public string? Complement { get; init; }
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;

    public string AdminName { get; init; } = string.Empty;
    public string AdminEmail { get; init; } = string.Empty;
    public string AdminDocumentNumber { get; init; } = string.Empty; 
    public string AdminPhoneNumber { get; init; } = string.Empty;
    public DateOnly AdminBirthDate { get; init; }
}