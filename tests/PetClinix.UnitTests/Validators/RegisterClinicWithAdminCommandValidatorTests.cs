using FluentAssertions;
using FluentValidation.TestHelper;
using PetClinix.Modules.Identity.Application.UseCases.RegisterClinicWithAdmin;
using Xunit;

namespace PetClinix.UnitTests.Validators;

public class RegisterClinicWithAdminCommandValidatorTests
{
    private readonly RegisterClinicWithAdminCommandValidator _validator;

    public RegisterClinicWithAdminCommandValidatorTests()
    {
        _validator = new RegisterClinicWithAdminCommandValidator();
    }

    private static RegisterClinicWithAdminCommand CreateValidCommand() => new()
    {
        TradeName = "Pet Love",
        LegalName = "Pet Love LTDA",
        DocumentNumber = "12345678000199",
        Email = "contato@petlove.com",
        PhoneNumber = "11988887777",
        ZipCode = "01001000",
        Street = "Rua Teste",
        Number = "123",
        Neighborhood = "Centro",
        City = "SP",
        State = "SP",
        AdminName = "Admin",
        AdminEmail = "admin@petlove.com",
        AdminDocumentNumber = "12345678900",
        AdminPhoneNumber = "11999990000",
        AdminBirthDate = new DateOnly(1990, 1, 1)
    };

    [Fact]
    public void Should_Pass_When_Command_Is_Valid()
    {
        var command = CreateValidCommand();

        var result = _validator.TestValidate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Fail_When_TradeName_Is_Empty()
    {
        var command = CreateValidCommand() with { TradeName = "" };

        var result = _validator.TestValidate(command);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.TradeName)
              .WithErrorMessage("O nome fantasia da clínica é obrigatório.");
    }

    [Fact]
    public void Should_Fail_When_DocumentNumber_Is_Empty()
    {
        var command = CreateValidCommand() with { DocumentNumber = "" };

        var result = _validator.TestValidate(command);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.DocumentNumber)
              .WithErrorMessage("O CNPJ da clínica é obrigatório.");
    }

    [Fact]
    public void Should_Fail_When_AdminBirthDate_Is_In_The_Future()
    {
        var command = CreateValidCommand() with { AdminBirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) };

        var result = _validator.TestValidate(command);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.AdminBirthDate);
    }

    [Fact]
    public void Should_Pass_When_Complement_Is_Empty()
    {
        var command = CreateValidCommand() with { Complement = null };

        var result = _validator.TestValidate(command);

        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveValidationErrorFor(x => x.Complement);
    }
}