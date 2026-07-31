using FluentAssertions;
using PetClinix.Modules.Identity.Domain.Entities;
using PetClinix.Modules.Identity.Domain.Enums;
using PetClinix.Modules.Identity.Domain.Exceptions;
using System;
using Xunit;

namespace PetClinix.UnitTests.Domain.Entities;

public class UserTests
{
    [Fact]
    public void CreateAdmin_Should_Throw_Exception_When_ClinicId_Is_Empty()
    {
        Action act = () => User.CreateAdmin(
            Guid.Empty,
            "Admin",
            "admin@pet.com",
            "hash",
            "12345678900",
            "11999990000",
            new DateOnly(1990, 1, 1));

        act.Should().Throw<IdentityDomainException>()
           .WithMessage("*A clínica do usuário é obrigatória.*");
    }

    [Fact]
    public void CreateAdmin_Should_Throw_Exception_When_Name_Is_Empty()
    {
        Action act = () => User.CreateAdmin(
            Guid.NewGuid(),
            "",
            "admin@pet.com",
            "hash",
            "12345678900",
            "11999990000",
            new DateOnly(1990, 1, 1));

        act.Should().Throw<IdentityDomainException>()
           .WithMessage("*O nome do usuário é obrigatório.*");
    }

    [Fact]
    public void CreateAdmin_Should_Set_Role_To_Admin_And_Active_To_True()
    {
        var user = User.CreateAdmin(
            Guid.NewGuid(),
            "Admin",
            "admin@pet.com",
            "hash",
            "12345678900",
            "11999990000",
            new DateOnly(1990, 1, 1));

        user.Role.Should().Be(UserRole.Admin);
        user.IsActive.Should().BeTrue();
        user.PasswordHash.Should().Be("hash");
    }

    [Fact]
    public void CreateStaff_Should_Throw_Exception_When_Role_Is_Admin()
    {
        Action act = () => User.CreateStaff(
            Guid.NewGuid(),
            "Staff",
            "staff@pet.com",
            "hash",
            "12345678900",
            "11999990000",
            new DateOnly(1990, 1, 1),
            UserRole.Admin);

        act.Should().Throw<IdentityDomainException>()
           .WithMessage("*Use a criação de administrador para cadastrar um usuário administrador.*");
    }

    [Fact]
    public void Deactivate_Should_Set_IsActive_To_False()
    {
        var user = User.CreateAdmin(
            Guid.NewGuid(),
            "Admin",
            "admin@pet.com",
            "hash",
            "12345678900",
            "11999990000",
            new DateOnly(1990, 1, 1));

        user.Deactivate();

        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void GeneratePasswordResetToken_Should_Set_Token_And_Expiry()
    {
        var user = User.CreateAdmin(
            Guid.NewGuid(), "Admin", "admin@pet.com", null,
            "12345678900", "11999990000", new DateOnly(1990, 1, 1));

        var token = user.GeneratePasswordResetToken();

        token.Should().NotBeNullOrEmpty();
        user.PasswordResetToken.Should().Be(token);
        user.PasswordResetTokenExpiresAtUtc.Should().NotBeNull();
        user.PasswordResetTokenExpiresAtUtc.Should().BeCloseTo(DateTime.UtcNow.AddHours(24), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SetPassword_Should_Throw_Exception_When_Token_Is_Invalid()
    {
        var user = User.CreateAdmin(
            Guid.NewGuid(), "Admin", "admin@pet.com", null,
            "12345678900", "11999990000", new DateOnly(1990, 1, 1));

        user.GeneratePasswordResetToken();

        Action act = () => user.SetPassword("token-errado", "hash_senha");

        act.Should().Throw<IdentityDomainException>()
           .WithMessage("*Token da redefinição inválido.*");
    }

    [Fact]
    public void SetPassword_Should_Set_Hash_And_Clear_Token_When_Valid()
    {
        var user = User.CreateAdmin(
            Guid.NewGuid(), "Admin", "admin@pet.com", null,
            "12345678900", "11999990000", new DateOnly(1990, 1, 1));

        var token = user.GeneratePasswordResetToken();

        user.SetPassword(token, "novo_hash_123");

        user.PasswordHash.Should().Be("novo_hash_123");
        user.PasswordResetToken.Should().BeNull();
        user.PasswordResetTokenExpiresAtUtc.Should().BeNull();
    }
}