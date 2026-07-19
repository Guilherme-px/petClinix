using FluentAssertions;
using PetClinix.Modules.Identity.Domain.Exceptions;
using PetClinix.Modules.Identity.Domain.ValueObjects;
using System;
using Xunit;

namespace PetClinix.UnitTests.Domain.ValueObjects;

public class ClinicSlugTests
{
    [Theory]
    [InlineData("Pet Love", "pet-love")]
    [InlineData("Pet Love & Cia", "pet-love-cia")]
    [InlineData("  Clínica  Vet   ", "clinica-vet")]
    [InlineData("Pet---Love", "pet-love")]
    [InlineData("PET@CÃO", "petcao")]
    public void CreateFromName_Should_Normalize_And_Remove_Accents(string input, string expected)
    {
        var slug = ClinicSlug.CreateFromName(input);

        slug.Value.Should().Be(expected);
    }

    [Fact]
    public void CreateFromName_Should_Throw_Exception_When_Name_Is_Empty()
    {
        Action act = () => ClinicSlug.CreateFromName("");

        act.Should().Throw<IdentityDomainException>()
           .WithMessage("*O nome fantasia é obrigatório para gerar o identificador.*");
    }

    [Fact]
    public void CreateFromName_Should_Throw_Exception_When_Result_Is_Too_Short()
    {
        Action act = () => ClinicSlug.CreateFromName("A");

        act.Should().Throw<IdentityDomainException>()
           .WithMessage("*O identificador da clínica deve ter entre 3 e 50 caracteres.*");
    }

    [Theory]
    [InlineData("valid-slug")]
    [InlineData("pet-123")]
    [InlineData("a-b-c")]
    public void Create_Should_Return_Valid_Slug_When_Input_Matches_Regex(string input)
    {
        var slug = ClinicSlug.Create(input);

        slug.Value.Should().Be(input);
    }

    [Theory]
    [InlineData("Invalid Slug")]
    [InlineData("invalid_slug")]
    [InlineData("invalid.slug")]
    public void Create_Should_Throw_Exception_When_Input_Has_Invalid_Characters(string input)
    {
        Action act = () => ClinicSlug.Create(input);

        act.Should().Throw<IdentityDomainException>()
           .WithMessage("*O identificador da clínica deve conter apenas letras minúsculas, números e hífens.*");
    }

    [Fact]
    public void Implicit_Conversion_Should_Return_String_Value()
    {
        var slug = ClinicSlug.Create("pet-love");

        string slugString = slug;

        slugString.Should().Be("pet-love");
    }
}