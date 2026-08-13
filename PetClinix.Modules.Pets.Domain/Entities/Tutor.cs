using PetClinix.BuildingBlocks.Domain;
using PetClinix.Modules.Pets.Domain.Exceptions;
using PetClinix.Modules.Pets.Domain.ValueObjects;

namespace PetClinix.Modules.Pets.Domain.Entities;

public sealed class Tutor : AggregateRoot
{
    public Guid ClinicId { get; private set; }
    public string Name { get; private set; }
    public Cpf Cpf { get; private set; }
    public string? Email { get; private set; }
    public string PhoneNumber { get; private set; }
    public string? SecondaryPhoneNumber { get; private set; }
    public string ZipCode { get; private set; }
    public string Street { get; private set; }
    public string Number { get; private set; }
    public string Neighborhood { get; private set; }
    public string? Complement { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

#pragma warning disable CS8618
    private Tutor() { }

    private Tutor(
       Guid clinicId, string name, Cpf cpf, string? email, string phoneNumber, string? secondaryPhoneNumber,
       string zipCode, string street, string number, string neighborhood, string? complement, string city, string state, string? notes)
    {
        if (clinicId == Guid.Empty) throw new PetsDomainException("pets.tutor.clinic_id_required", "Clínica é obrigatória.");
        if (string.IsNullOrWhiteSpace(name)) throw new PetsDomainException("pets.tutor.name_required", "Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(phoneNumber)) throw new PetsDomainException("pets.tutor.phone_required", "Telefone é obrigatório.");
        if (string.IsNullOrWhiteSpace(zipCode)) throw new PetsDomainException("pets.tutor.zipcode_required", "CEP é obrigatório.");
        if (string.IsNullOrWhiteSpace(street)) throw new PetsDomainException("pets.tutor.street_required", "Rua é obrigatória.");
        if (string.IsNullOrWhiteSpace(number)) throw new PetsDomainException("pets.tutor.number_required", "Número é obrigatório.");
        if (string.IsNullOrWhiteSpace(neighborhood)) throw new PetsDomainException("pets.tutor.neighborhood_required", "Bairro é obrigatório.");
        if (string.IsNullOrWhiteSpace(city)) throw new PetsDomainException("pets.tutor.city_required", "Cidade é obrigatória.");
        if (string.IsNullOrWhiteSpace(state)) throw new PetsDomainException("pets.tutor.state_required", "Estado é obrigatório.");

        Id = Guid.NewGuid();
        ClinicId = clinicId;
        Name = name.Trim();
        Cpf = cpf;
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        PhoneNumber = phoneNumber.Trim();
        SecondaryPhoneNumber = string.IsNullOrWhiteSpace(secondaryPhoneNumber) ? null : secondaryPhoneNumber.Trim();
        ZipCode = zipCode.Trim();
        Street = street.Trim();
        Number = number.Trim();
        Neighborhood = neighborhood.Trim();
        Complement = string.IsNullOrWhiteSpace(complement) ? null : complement.Trim();
        City = city.Trim();
        State = state.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Tutor Create(
        Guid clinicId, string name, string cpf, string? email, string phoneNumber, string? secondaryPhoneNumber,
        string zipCode, string street, string number, string neighborhood, string? complement, string city, string state, string? notes
    )
    {
        return new Tutor(
            clinicId, name, Cpf.Create(cpf), email, phoneNumber, secondaryPhoneNumber,
            zipCode, street, number, neighborhood, complement, city, state, notes
        );
    }

    public void Deactivate() => IsActive = false;
}