namespace PetClinix.Modules.Identity.Domain.Exceptions;

public sealed class IdentityDomainException : Exception
{
    public string Code { get; }

    public IdentityDomainException(string code, string message) : base(message)
    {
        Code = code;
    }
}