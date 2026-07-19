using FluentAssertions;
using PetClinix.Modules.Identity.Domain.Entities;
using PetClinix.Modules.Identity.Domain.Enums;
using PetClinix.Modules.Identity.Domain.Exceptions;
using PetClinix.Modules.Identity.Domain.ValueObjects;
using System;
using Xunit;

namespace PetClinix.UnitTests.Domain.Entities;

public class ClinicTests
{
    private static Clinic CreateValidClinic()
    {
        return Clinic.Create(
            "Pet Love",
            "Pet Love LTDA",
            "12345678000199",
            ClinicSlug.Create("pet-love"),
            "contato@petlove.com",
            "11988887777",
            "01001000",
            "Rua Teste",
            "123",
            "Centro",
            null,
            "SP",
            "SP");
    }

    [Fact]
    public void Create_Should_Throw_Exception_When_TradeName_Is_Empty()
    {
        Action act = () => Clinic.Create(
            "",
            "Pet Love LTDA",
            "12345678000199",
            ClinicSlug.Create("pet-love"),
            "contato@petlove.com",
            "11988887777",
            "01001000",
            "Rua Teste",
            "123",
            "Centro",
            null,
            "SP",
            "SP");

        act.Should().Throw<IdentityDomainException>()
           .WithMessage("*O nome fantasia da clínica é obrigatório.*");
    }

    [Fact]
    public void Create_Should_Set_Status_To_Active_By_Default()
    {
        var clinic = CreateValidClinic();

        clinic.Status.Should().Be(ClinicStatus.Active);
        clinic.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Deactivate_Should_Set_Status_To_Inactive()
    {
        var clinic = CreateValidClinic();

        clinic.Deactivate();

        clinic.Status.Should().Be(ClinicStatus.Inactive);
    }

    [Fact]
    public void UpdateContactInfo_Should_Change_Email_And_Phone()
    {
        var clinic = CreateValidClinic();

        clinic.UpdateContactInfo("novo@email.com", "1188887777");

        clinic.Email.Value.Should().Be("novo@email.com");
        clinic.PhoneNumber.Value.Should().Be("1188887777");
    }
}