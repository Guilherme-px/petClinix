using System.Text.RegularExpressions;
using PetClinix.Modules.Identity.Domain.Exceptions;

namespace PetClinix.Modules.Identity.Domain.ValueObjects;

public sealed class Email
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new IdentityDomainException(
                "identity.email.required",
                "E-mail é obrigatório.");

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > 256)
            throw new IdentityDomainException(
                "identity.email.invalid_length",
                "E-mail inválido.");

        if (!EmailRegex.IsMatch(normalized))
            throw new IdentityDomainException(
                "identity.email.invalid",
                "E-mail inválido.");

        return new Email(normalized);
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}