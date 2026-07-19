using FluentAssertions;
using PetClinix.Modules.Identity.Domain.Exceptions;
using PetClinix.Modules.Identity.Domain.ValueObjects;
using System;
using Xunit;

namespace PetClinix.UnitTests.Domain.ValueObjects;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("11988887777")]
    [InlineData("(11) 98888-7777")]
    [InlineData("+55 11 98888 7777")]
    public void Create_Should_Accept_Valid_Phone_Numbers(string input)
    {
        var phone = PhoneNumber.Create(input);

        phone.Value.Should().Be(input.Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_Should_Throw_Exception_When_Empty(string input)
    {
        Action act = () => PhoneNumber.Create(input);

        act.Should().Throw<IdentityDomainException>()
           .WithMessage("*O telefone é obrigatório.*");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("11")]
    public void Create_Should_Throw_Exception_When_Too_Short(string input)
    {
        Action act = () => PhoneNumber.Create(input);

        act.Should().Throw<IdentityDomainException>()
           .WithMessage("*O telefone informado é inválido.*");
    }

    [Theory]
    [InlineData("1198888ABCD")]
    [InlineData("email@pet.com")]
    public void Create_Should_Throw_Exception_When_Has_Invalid_Characters(string input)
    {
        Action act = () => PhoneNumber.Create(input);

        act.Should().Throw<IdentityDomainException>()
           .WithMessage("*O telefone informado é inválido.*");
    }
}