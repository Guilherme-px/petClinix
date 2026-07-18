using PetClinix.Modules.Identity.Application.Contracts;

namespace PetClinix.Modules.Identity.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }
}