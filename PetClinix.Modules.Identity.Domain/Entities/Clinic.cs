using PetClinix.BuildingBlocks.Domain;
using PetClinix.Modules.Identity.Domain.Enums;
using PetClinix.Modules.Identity.Domain.Exceptions;
using PetClinix.Modules.Identity.Domain.ValueObjects;

namespace PetClinix.Modules.Identity.Domain.Entities;

public sealed class Clinic : AggregateRoot
{
    public string TradeName { get; private set; }
    public string LegalName { get; private set; } 
    public string DocumentNumber { get; private set; }
    public ClinicSlug Slug { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public string ZipCode { get; private set; }
    public string Street { get; private set; }
    public string Number { get; private set; }
    public string Neighborhood { get; private set; }
    public string? Complement { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public ClinicStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Clinic(
        string tradeName, string legalName, string documentNumber, ClinicSlug slug,
        Email email, PhoneNumber phoneNumber, string zipCode, string street, string number,
        string neighborhood, string? complement, string city, string state)
    {
        if (string.IsNullOrWhiteSpace(tradeName))
            throw new IdentityDomainException("identity.clinic.trade_name_required", "O nome fantasia da clínica é obrigatório.");
        if (string.IsNullOrWhiteSpace(legalName))
            throw new IdentityDomainException("identity.clinic.legal_name_required", "A razão social da clínica é obrigatória.");
        if (string.IsNullOrWhiteSpace(documentNumber))
            throw new IdentityDomainException("identity.clinic.document_required", "O CNPJ da clínica é obrigatório.");
        if (string.IsNullOrWhiteSpace(zipCode))
            throw new IdentityDomainException("identity.clinic.zipcode_required", "O CEP da clínica é obrigatório.");
        if (string.IsNullOrWhiteSpace(street))
            throw new IdentityDomainException("identity.clinic.street_required", "A rua da clínica é obrigatória.");
        if (string.IsNullOrWhiteSpace(number))
            throw new IdentityDomainException("identity.clinic.number_required", "O número da clínica é obrigatório.");
        if (string.IsNullOrWhiteSpace(neighborhood))
            throw new IdentityDomainException("identity.clinic.neighborhood_required", "O bairro da clínica é obrigatório.");
        if (string.IsNullOrWhiteSpace(city))
            throw new IdentityDomainException("identity.clinic.city_required", "A cidade da clínica é obrigatória.");
        if (string.IsNullOrWhiteSpace(state))
            throw new IdentityDomainException("identity.clinic.state_required", "O estado da clínica é obrigatório.");

        Id = Guid.NewGuid();
        TradeName = tradeName.Trim();
        LegalName = legalName.Trim();
        DocumentNumber = documentNumber.Trim();
        Slug = slug;
        Email = email;
        PhoneNumber = phoneNumber;
        ZipCode = zipCode.Trim();
        Street = street.Trim();
        Number = number.Trim();
        Neighborhood = neighborhood.Trim();
        Complement = string.IsNullOrWhiteSpace(complement) ? null : complement.Trim();
        City = city.Trim();
        State = state.Trim();
        Status = ClinicStatus.Active;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Clinic Create(
        string tradeName, string legalName, string documentNumber, ClinicSlug slug,
        string email, string phoneNumber, string zipCode, string street, string number,
        string neighborhood, string? complement, string city, string state)
    {
        return new Clinic(
            tradeName, 
            legalName, 
            documentNumber, 
            slug, 
            Email.Create(email), 
            PhoneNumber.Create(phoneNumber),
            zipCode, 
            street, 
            number, 
            neighborhood, 
            complement, 
            city, 
            state);
    }

    public void Deactivate() => Status = ClinicStatus.Inactive;
    public void Activate() => Status = ClinicStatus.Active;

    public void UpdateContactInfo(string email, string phoneNumber)
    {
        Email = Email.Create(email);
        PhoneNumber = PhoneNumber.Create(phoneNumber);
    }
}