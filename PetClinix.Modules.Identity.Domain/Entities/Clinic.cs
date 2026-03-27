using PetClinix.BuildingBlocks.Domain;
using PetClinix.Modules.Identity.Domain.Enums;
using PetClinix.Modules.Identity.Domain.Exceptions;
using PetClinix.Modules.Identity.Domain.ValueObjects;

namespace PetClinix.Modules.Identity.Domain.Entities;

public sealed class Clinic : AggregateRoot
{
    public string TradeName { get; private set; }
    public string? LegalName { get; private set; }
    public string? DocumentNumber { get; private set; }
    public ClinicSlug Slug { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public string? AddressLine { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? ZipCode { get; private set; }
    public ClinicStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Clinic(
        string tradeName,
        string? legalName,
        string? documentNumber,
        ClinicSlug slug,
        Email email,
        PhoneNumber phoneNumber,
        string? addressLine,
        string? city,
        string? state,
        string? zipCode)
    {
        if (string.IsNullOrWhiteSpace(tradeName))
            throw new IdentityDomainException(
                "identity.clinic.trade_name_required",
                "O nome fantasia da clínica é obrigatório.");

        Id = Guid.NewGuid();
        TradeName = tradeName.Trim();
        LegalName = string.IsNullOrWhiteSpace(legalName) ? null : legalName.Trim();
        DocumentNumber = string.IsNullOrWhiteSpace(documentNumber) ? null : documentNumber.Trim();
        Slug = slug;
        Email = email;
        PhoneNumber = phoneNumber;
        AddressLine = string.IsNullOrWhiteSpace(addressLine) ? null : addressLine.Trim();
        City = string.IsNullOrWhiteSpace(city) ? null : city.Trim();
        State = string.IsNullOrWhiteSpace(state) ? null : state.Trim();
        ZipCode = string.IsNullOrWhiteSpace(zipCode) ? null : zipCode.Trim();
        Status = ClinicStatus.Active;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Clinic Create(
        string tradeName,
        string? legalName,
        string? documentNumber,
        string slug,
        string email,
        string phoneNumber,
        string? addressLine = null,
        string? city = null,
        string? state = null,
        string? zipCode = null)
    {
        return new Clinic(
            tradeName,
            legalName,
            documentNumber,
            ClinicSlug.Create(slug),
            Email.Create(email),
            PhoneNumber.Create(phoneNumber),
            addressLine,
            city,
            state,
            zipCode);
    }

    public void Deactivate()
    {
        Status = ClinicStatus.Inactive;
    }

    public void Activate()
    {
        Status = ClinicStatus.Active;
    }

    public void UpdateContactInfo(string email, string phoneNumber)
    {
        Email = Email.Create(email);
        PhoneNumber = PhoneNumber.Create(phoneNumber);
    }

    public void UpdateAddress(
        string? addressLine,
        string? city,
        string? state,
        string? zipCode)
    {
        AddressLine = string.IsNullOrWhiteSpace(addressLine) ? null : addressLine.Trim();
        City = string.IsNullOrWhiteSpace(city) ? null : city.Trim();
        State = string.IsNullOrWhiteSpace(state) ? null : state.Trim();
        ZipCode = string.IsNullOrWhiteSpace(zipCode) ? null : zipCode.Trim();
    }
}