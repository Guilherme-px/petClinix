namespace PetClinix.Modules.Pets.Domain.Exceptions;

public sealed class PetsDomainException : Exception
{
    public string Code { get; }
    public PetsDomainException(string code, string message) : base(message) { Code = code; }
}