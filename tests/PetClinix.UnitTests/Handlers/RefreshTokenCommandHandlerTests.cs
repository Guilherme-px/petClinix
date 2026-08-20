using FluentAssertions;
using NSubstitute;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Application.Contracts;
using PetClinix.Modules.Identity.Application.UseCases.RefreshToken;
using PetClinix.Modules.Identity.Domain.Entities;
using PetClinix.Modules.Identity.Domain.Repositories;
using System;
using Xunit;

namespace PetClinix.UnitTests.Handlers;

public class RefreshTokenCommandHandlerTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IJwtTokenGenerator _jwtTokenGeneratorMock;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _jwtTokenGeneratorMock = Substitute.For<IJwtTokenGenerator>();
        _handler = new RefreshTokenCommandHandler(_userRepositoryMock, _jwtTokenGeneratorMock);
    }

    private static User CreateValidUser()
    {
        return User.CreateAdmin(
            Guid.NewGuid(), Guid.NewGuid(), "Admin", "admin@test.com", "hash",
            "12345678900", "11999990000", new DateOnly(1990, 1, 1)
        );
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_Token_Does_Not_Exist()
    {
        var command = new RefreshTokenCommand("invalid-token");
        _userRepositoryMock.GetByRefreshTokenAsync(command.RefreshToken, Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("auth.invalid_refresh_token");
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_Token_Is_Expired()
    {
        var user = CreateValidUser();
        var token = user.GenerateRefreshToken();

        typeof(User).GetProperty("RefreshTokenExpiresAtUtc")!.SetValue(user, DateTime.UtcNow.AddHours(-1));

        var command = new RefreshTokenCommand(token);
        _userRepositoryMock.GetByRefreshTokenAsync(token, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("auth.invalid_refresh_token");
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_And_NewTokens_When_Valid()
    {
        var user = CreateValidUser();
        var oldToken = user.GenerateRefreshToken();

        var command = new RefreshTokenCommand(oldToken);
        _userRepositoryMock.GetByRefreshTokenAsync(oldToken, Arg.Any<CancellationToken>()).Returns(user);
        _jwtTokenGeneratorMock.GenerateToken(user).Returns("new.jwt.token");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Token.Should().Be("new.jwt.token");
        result.Value.RefreshToken.Should().NotBeNullOrEmpty();
        result.Value.RefreshToken.Should().NotBe(oldToken);

        await _userRepositoryMock.Received(1).UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }
}