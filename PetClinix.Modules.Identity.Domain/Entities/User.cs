using PetClinix.BuildingBlocks.Domain;
using PetClinix.Modules.Identity.Domain.Enums;
using PetClinix.Modules.Identity.Domain.Exceptions;
using PetClinix.Modules.Identity.Domain.ValueObjects;

namespace PetClinix.Modules.Identity.Domain.Entities;

public sealed class User : AggregateRoot
{
    public Guid ClinicId { get; private set; }
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private User(
        Guid clinicId,
        string name,
        Email email,
        string passwordHash,
        UserRole role)
    {
        if (clinicId == Guid.Empty)
            throw new IdentityDomainException(
                "identity.user.clinic_id_required",
                "A clínica do usuário é obrigatória.");

        if (string.IsNullOrWhiteSpace(name))
            throw new IdentityDomainException(
                "identity.user.name_required",
                "O nome do usuário é obrigatório.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new IdentityDomainException(
                "identity.user.password_hash_required",
                "A senha do usuário é obrigatória.");

        Id = Guid.NewGuid();
        ClinicId = clinicId;
        Name = name.Trim();
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static User CreateAdmin(
        Guid clinicId,
        string name,
        string email,
        string passwordHash)
    {
        return new User(
            clinicId,
            name,
            Email.Create(email),
            passwordHash,
            UserRole.Admin);
    }

    public static User CreateStaff(
        Guid clinicId,
        string name,
        string email,
        string passwordHash,
        UserRole role)
    {
        if (role == UserRole.Admin)
            throw new IdentityDomainException(
                "identity.user.invalid_staff_role",
                "Use a criação de administrador para cadastrar um usuário administrador.");

        return new User(
            clinicId,
            name,
            Email.Create(email),
            passwordHash,
            role);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new IdentityDomainException(
                "identity.user.name_required",
                "O nome do usuário é obrigatório.");

        Name = name.Trim();
    }
}