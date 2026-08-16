using PetClinix.BuildingBlocks.Domain;
using PetClinix.Modules.Pets.Domain.Enums;
using PetClinix.Modules.Pets.Domain.Exceptions;

namespace PetClinix.Modules.Pets.Domain.Entities;

public sealed class Pet : AggregateRoot
{
    public Guid ClinicId { get; private set; }
    public Guid TutorId { get; private set; }
    public string Name { get; private set; }
    public Species Species { get; private set; }
    public string? Breed { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public PetSex Sex { get; private set; }
    public double? Weight { get; private set; }
    public bool IsNeutered { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }

#pragma warning disable CS8618
    private Pet() { }

    private Pet(Guid clinicId, Guid tutorId, Guid createdByUserId, string name, Species species,
        string? breed, DateOnly? birthDate, PetSex sex, double? weight, bool isNeutered, string? notes)
    {
        if (clinicId == Guid.Empty) throw new PetsDomainException("pets.pet.clinic_id_required", "Clínica é obrigatória.");
        if (tutorId == Guid.Empty) throw new PetsDomainException("pets.pet.tutor_id_required", "Tutor é obrigatório.");
        if (createdByUserId == Guid.Empty) throw new PetsDomainException("pets.pet.created_by_required", "Usuário criador é obrigatório.");
        if (string.IsNullOrWhiteSpace(name)) throw new PetsDomainException("pets.pet.name_required", "Nome do pet é obrigatório.");

        Id = Guid.NewGuid();
        ClinicId = clinicId;
        TutorId = tutorId;
        CreatedByUserId = createdByUserId;
        Name = name.Trim();
        Species = species;
        Breed = string.IsNullOrWhiteSpace(breed) ? null : breed.Trim();
        BirthDate = birthDate;
        Sex = sex;
        Weight = weight;
        IsNeutered = isNeutered;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Pet Create(Guid clinicId, Guid tutorId, Guid createdByUserId, string name, Species species,
        string? breed, DateOnly? birthDate, PetSex sex, double? weight, bool isNeutered, string? notes)
    {
        return new Pet(clinicId, tutorId, createdByUserId, name, species, breed, birthDate, sex, weight, isNeutered, notes);
    }

    public void UpdateInfo(Guid updatedByUserId, string name, Species species, string? breed,
        DateOnly? birthDate, PetSex sex, double? weight, bool isNeutered, string? notes)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new PetsDomainException("pets.pet.name_required", "Nome do pet é obrigatório.");
        }

        Name = name.Trim();
        Species = species;
        Breed = string.IsNullOrWhiteSpace(breed) ? null : breed.Trim();
        BirthDate = birthDate;
        Sex = sex;
        Weight = weight;
        IsNeutered = isNeutered;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate() => IsActive = false;
}