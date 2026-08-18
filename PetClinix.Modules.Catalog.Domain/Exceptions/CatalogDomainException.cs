namespace PetClinix.Modules.Catalog.Domain.Exceptions;

public sealed class CatalogDomainException : Exception
{
    public string Code { get; }
    public CatalogDomainException(string code, string message) : base(message) { Code = code; }
}