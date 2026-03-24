using PetClinix.Modules.Identity.Domain.Enums;

namespace PetClinix.Modules.Identity.Domain.Entities;

public class Clinic
{
    public Guid Id { get; private set; }
    public string TradeName { get; private set; }
    public string? LegalName { get; private set; }
    public string? DocumentNumber { get; private set; }
    public string Slug { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }
    public string? AddressLine { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? ZipCode { get; private set; }
    public ClinicStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public Clinic(
        string tradeName,
        string slug,
        string email,
        string phoneNumber,
        string? legalName = null,
        string? documentNumber = null,
        string? addressLine = null,
        string? city = null,
        string? state = null,
        string? zipCode = null)
    {
        Id = Guid.NewGuid();
        TradeName = tradeName;
        Slug = slug;
        Email = email;
        PhoneNumber = phoneNumber;
        LegalName = legalName;
        DocumentNumber = documentNumber;
        AddressLine = addressLine;
        City = city;
        State = state;
        ZipCode = zipCode;
        Status = ClinicStatus.Active;
        CreatedAtUtc = DateTime.UtcNow;
    }
}