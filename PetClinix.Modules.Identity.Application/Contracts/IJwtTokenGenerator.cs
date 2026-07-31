using PetClinix.Modules.Identity.Domain.Entities;

namespace PetClinix.Modules.Identity.Application.Contracts;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}