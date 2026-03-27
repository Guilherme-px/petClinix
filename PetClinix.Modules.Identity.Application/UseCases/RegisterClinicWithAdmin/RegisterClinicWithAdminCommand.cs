using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Identity.Application.UseCases.RegisterClinicWithAdmin;

public sealed class RegisterClinicWithAdminCommand : ICommand<Result<RegisterClinicWithAdminResponse>>
{
    public string TradeName { get; init; } = string.Empty;
    public string? LegalName { get; init; }
    public string? DocumentNumber { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? AddressLine { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? ZipCode { get; init; }

    public string AdminName { get; init; } = string.Empty;
    public string AdminEmail { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}