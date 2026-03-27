using System.Text.RegularExpressions;
using PetClinix.Modules.Identity.Domain.Exceptions;

namespace PetClinix.Modules.Identity.Domain.ValueObjects;

public sealed class PhoneNumber
{
    private static readonly Regex AllowedCharactersRegex = new(
        @"^[0-9()\-\s+]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new IdentityDomainException(
                "identity.phone.required",
                "O telefone é obrigatório.");

        var normalized = value.Trim();

        if (normalized.Length < 8 || normalized.Length > 20)
            throw new IdentityDomainException(
                "identity.phone.invalid_length",
                "O telefone informado é inválido.");

        if (!AllowedCharactersRegex.IsMatch(normalized))
            throw new IdentityDomainException(
                "identity.phone.invalid",
                "O telefone informado é inválido.");

        return new PhoneNumber(normalized);
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
}