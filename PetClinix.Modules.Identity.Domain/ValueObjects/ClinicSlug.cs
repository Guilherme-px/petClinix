using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using PetClinix.Modules.Identity.Domain.Exceptions;

namespace PetClinix.Modules.Identity.Domain.ValueObjects;

public sealed class ClinicSlug
{
    private static readonly Regex SlugRegex = new(
        @"^[a-z0-9]+(?:-[a-z0-9]+)*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; }

    private ClinicSlug(string value)
    {
        Value = value;
    }

    public static ClinicSlug Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new IdentityDomainException(
                "identity.clinic_slug.required",
                "O identificador da clínica é obrigatório.");

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length < 3 || normalized.Length > 50)
            throw new IdentityDomainException(
                "identity.clinic_slug.invalid_length",
                "O identificador da clínica deve ter entre 3 e 50 caracteres.");

        if (!SlugRegex.IsMatch(normalized))
            throw new IdentityDomainException(
                "identity.clinic_slug.invalid",
                "O identificador da clínica deve conter apenas letras minúsculas, números e hífens.");

        return new ClinicSlug(normalized);
    }

    public static ClinicSlug CreateFromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new IdentityDomainException(
                "identity.clinic_slug.required",
                "O nome fantasia é obrigatório para gerar o identificador.");

        string normalized = name.Trim().ToLowerInvariant();

        normalized = normalized.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (char c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        normalized = sb.ToString();

        normalized = Regex.Replace(normalized, @"[^a-z0-9\s-]", ""); 
        normalized = Regex.Replace(normalized, @"\s+", "-");         
        normalized = Regex.Replace(normalized, @"-+", "-");          
        normalized = normalized.Trim('-');                         

        return Create(normalized);
    }

    public override string ToString() => Value;

    public static implicit operator string(ClinicSlug slug) => slug.Value;
}