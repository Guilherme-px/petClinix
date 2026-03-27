namespace PetClinix.Modules.Identity.Application.Contracts;

public interface IPasswordHasher
{
    string Hash(string password);
}