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
}