using FluentAssertions;
using PetClinix.Modules.Identity.Domain.ValueObjects;
using PetClinix.Modules.Identity.Domain.Exceptions;
using Xunit;
using System;

namespace PetClinix.UnitTests.Domain.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("admin@pet.com")]
    [InlineData("ADMIN@PET.COM")]
    [InlineData("  admin@pet.com  ")]
    public void Create_Should_Normalize_And_Accept_Valid_Emails(string input)
    {
        var email = Email.Create(input);

        email.Value.Should().Be("admin@pet.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_Should_Throw_Exception_When_Empty(string input)
    {
        Action act = () => Email.Create(input);

        act.Should().Throw<IdentityDomainException>()
           .WithMessage("*E-mail é obrigatório.*");
    }

    [Theory]
    [InlineData("adminpet.com")]
    [InlineData("admin@pet")]
    [InlineData("admin@.com")]
    public void Create_Should_Throw_Exception_When_Invalid_Format(string input)
    {
        Action act = () => Email.Create(input);

        act.Should().Throw<IdentityDomainException>()
           .WithMessage("*E-mail inválido.*");
    }

    [Fact]
    public void Create_Should_Throw_Exception_When_Too_Long()
    {
        var longEmail = new string('a', 250) + "@pet.com";

        Action act = () => Email.Create(longEmail);

        act.Should().Throw<IdentityDomainException>()
           .WithMessage("*E-mail inválido.*");
    }
}