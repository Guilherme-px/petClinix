using PetClinix.BuildingBlocks.Domain;
using PetClinix.Modules.Catalog.Domain.Exceptions;

namespace PetClinix.Modules.Catalog.Domain.Entities;

public sealed class Service : AggregateRoot
{
    public Guid ClinicId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public int DurationInMinutes { get; private set; }
    public decimal Price { get; private set; }
    public bool RequiresVeterinarian { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }

#pragma warning disable CS8618
    private Service() { }

    private Service(
        Guid clinicId, Guid createdByUserId, string name, string? description,
        int durationInMinutes, decimal price, bool requiresVeterinarian)
    {
        if (clinicId == Guid.Empty) throw new CatalogDomainException("catalog.service.clinic_id_required", "Clínica é obrigatória.");
        if (createdByUserId == Guid.Empty) throw new CatalogDomainException("catalog.service.created_by_required", "Usuário criador é obrigatório.");
        if (string.IsNullOrWhiteSpace(name)) throw new CatalogDomainException("catalog.service.name_required", "Nome do serviço é obrigatório.");
        if (durationInMinutes <= 0) throw new CatalogDomainException("catalog.service.invalid_duration", "A duração deve ser maior que zero.");
        if (price < 0) throw new CatalogDomainException("catalog.service.invalid_price", "O preço não pode ser negativo.");

        Id = Guid.NewGuid();
        ClinicId = clinicId;
        CreatedByUserId = createdByUserId;
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        DurationInMinutes = durationInMinutes;
        Price = price;
        RequiresVeterinarian = requiresVeterinarian;

        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Service Create(
        Guid clinicId, Guid createdByUserId, string name, string? description,
        int durationInMinutes, decimal price, bool requiresVeterinarian)
    {
        return new Service(clinicId, createdByUserId, name, description, durationInMinutes, price, requiresVeterinarian);
    }

    public void UpdateInfo(
        Guid updatedByUserId, string name, string? description,
        int durationInMinutes, decimal price, bool requiresVeterinarian)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new CatalogDomainException("catalog.service.name_required", "Nome do serviço é obrigatório.");
        if (durationInMinutes <= 0) throw new CatalogDomainException("catalog.service.invalid_duration", "A duração deve ser maior que zero.");
        if (price < 0) throw new CatalogDomainException("catalog.service.invalid_price", "O preço não pode ser negativo.");

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        DurationInMinutes = durationInMinutes;
        Price = price;
        RequiresVeterinarian = requiresVeterinarian;
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate() => IsActive = false;
}