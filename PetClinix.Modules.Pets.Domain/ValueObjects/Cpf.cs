using System.Text.RegularExpressions;
using PetClinix.Modules.Pets.Domain.Exceptions;

namespace PetClinix.Modules.Pets.Domain.ValueObjects;

public sealed class Cpf
{
    private static readonly Regex CpfRegex = new(
        @"^\d{3}\.\d{3}\.\d{3}-\d{2}$|^\d{11}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; }
    private Cpf(string value) { Value = value; }

    public static Cpf Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new PetsDomainException("pets.tutor.cpf_required", "O CPF do tutor é obrigatório.");

        var normalized = value.Trim().Replace(".", "").Replace("-", "");

        if (normalized.Length != 11 || !normalized.All(char.IsDigit))
            throw new PetsDomainException("pets.tutor.invalid_cpf", "CPF inválido. Deve conter 11 dígitos.");

        return new Cpf(normalized);
    }
    public override string ToString() => Value;
    public static implicit operator string(Cpf cpf) => cpf.Value;
}
