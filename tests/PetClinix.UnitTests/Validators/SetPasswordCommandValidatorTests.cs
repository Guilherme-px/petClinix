using FluentAssertions;
using FluentValidation.TestHelper;
using PetClinix.Modules.Identity.Application.UseCases.SetPassword;
using Xunit;

namespace PetClinix.UnitTests.Validators;

public class SetPasswordCommandValidatorTests
{
    private readonly SetPasswordCommandValidator _validator;

    public SetPasswordCommandValidatorTests()
    {
        _validator = new SetPasswordCommandValidator();
    }

    private static SetPasswordCommand CreateValidCommand() => new("valid-token-123", "SenhaForte@123");

    [Fact]
    public void Should_Pass_When_Command_Is_Valid()
    {
        var command = CreateValidCommand();
        var result = _validator.TestValidate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Fail_When_Token_Is_Empty()
    {
        var command = CreateValidCommand() with { Token = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Theory]
    [InlineData("curta1@")] 
    [InlineData("senhasemaiuscula@1")] 
    [InlineData("SENHASEMCARACTEREESPECIAL1")] 
    public void Should_Fail_When_Password_Does_Not_Meet_Complexity_Rules(string password)
    {
        var command = CreateValidCommand() with { Password = password };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}