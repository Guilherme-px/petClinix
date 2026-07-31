using FluentAssertions;
using NSubstitute;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Application.Contracts;
using PetClinix.Modules.Identity.Application.UseCases.Login;
using PetClinix.Modules.Identity.Domain.Entities;
using PetClinix.Modules.Identity.Domain.Enums;
using PetClinix.Modules.Identity.Domain.Repositories;
using PetClinix.Modules.Identity.Domain.ValueObjects;
using System;
using Xunit;

namespace PetClinix.UnitTests.Handlers;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IPasswordHasher _passwordHasherMock;
    private readonly IJwtTokenGenerator _jwtTokenGeneratorMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _passwordHasherMock = Substitute.For<IPasswordHasher>();
        _jwtTokenGeneratorMock = Substitute.For<IJwtTokenGenerator>();

        _handler = new LoginCommandHandler(_userRepositoryMock, _passwordHasherMock, _jwtTokenGeneratorMock);
    }

    private static User CreateValidUser(string? passwordHash = "valid_hash")
    {
        var user = User.CreateAdmin(
            Guid.NewGuid(),
            "Admin Teste",
            "admin@teste.com",
            passwordHash,
            "12345678900",
            "11999990000",
            new DateOnly(1990, 1, 1));

        return user;
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_User_Does_Not_Exist()
    {
        var command = new LoginCommand("naoexiste@teste.com", "senha");
        _userRepositoryMock.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("auth.invalid_credentials");
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_PasswordHash_Is_Null()
    {
        var command = new LoginCommand("admin@teste.com", "senha");
        var user = CreateValidUser(passwordHash: null);

        _userRepositoryMock.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("auth.invalid_credentials");
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_Password_Is_Invalid()
    {
        var command = new LoginCommand("admin@teste.com", "senha_errada");
        var user = CreateValidUser("valid_hash");

        _userRepositoryMock.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasherMock.Verify(command.Password, user.PasswordHash!).Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("auth.invalid_credentials");
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_And_Token_When_Valid()
    {
        var command = new LoginCommand("admin@teste.com", "senha_correta");
        var user = CreateValidUser("valid_hash");
        var fakeToken = "meu.token.jwt.falso";

        _userRepositoryMock.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasherMock.Verify(command.Password, user.PasswordHash!).Returns(true);
        _jwtTokenGeneratorMock.GenerateToken(user).Returns(fakeToken);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Token.Should().Be(fakeToken);
        result.Value.Role.Should().Be(UserRole.Admin.ToString());

        _jwtTokenGeneratorMock.Received(1).GenerateToken(user);
        await _userRepositoryMock.Received(1).UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }
}