namespace PetClinix.Modules.Identity.Domain.Entities;
using PetClinix.Modules.Identity.Domain.Enums;

public class User
{
    public Guid Id { get; private set; }
    public Guid ClinicId { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public User(Guid clinicId, string name, string email, string passwordHash, UserRole role)
    {
        Id = Guid.NewGuid();
        ClinicId = clinicId;
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }
}