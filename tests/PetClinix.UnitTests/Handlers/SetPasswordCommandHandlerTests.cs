using FluentAssertions;
using NSubstitute;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Application.Contracts;
using PetClinix.Modules.Identity.Application.UseCases.SetPassword;
using PetClinix.Modules.Identity.Domain.Entities;
using PetClinix.Modules.Identity.Domain.Exceptions;
using PetClinix.Modules.Identity.Domain.Repositories;
using PetClinix.Modules.Identity.Domain.ValueObjects;
using System;
using Xunit;

namespace PetClinix.UnitTests.Handlers;

public class SetPasswordCommandHandlerTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IPasswordHasher _passwordHasherMock;
    private readonly SetPasswordCommandHandler _handler;
    private readonly IUnitOfWork _unitOfWorkMock;

    public SetPasswordCommandHandlerTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _passwordHasherMock = Substitute.For<IPasswordHasher>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _handler = new SetPasswordCommandHandler(_userRepositoryMock, _passwordHasherMock, _unitOfWorkMock);
    }

    private static SetPasswordCommand CreateValidCommand() => new("valid-token-123", "NovaSenhaSegura123");

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_Token_Is_Invalid()
    {
        var command = CreateValidCommand();

        _userRepositoryMock
            .GetByPasswordResetTokenAsync(command.Token, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("identity.user.invalid_token");
        await _userRepositoryMock.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_Domain_Throws_Expiration_Error()
    {
        var command = CreateValidCommand();
        var user = User.CreateAdmin(Guid.NewGuid(), Guid.NewGuid(), "Admin", "admin@test.com", null, "12345678900", "11999990000", new DateOnly(1990, 1, 1));

        typeof(User).GetProperty("PasswordResetToken")!.SetValue(user, command.Token);
        typeof(User).GetProperty("PasswordResetTokenExpiresAtUtc")!.SetValue(user, DateTime.UtcNow.AddHours(-1)); 

        _userRepositoryMock
            .GetByPasswordResetTokenAsync(command.Token, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("identity.user.expired_token");
        await _userRepositoryMock.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_And_Update_User_When_Valid()
    {
        var command = CreateValidCommand();
        var user = User.CreateAdmin(Guid.NewGuid(), Guid.NewGuid(), "Admin", "admin@test.com", null, "12345678900", "11999990000", new DateOnly(1990, 1, 1));

        typeof(User).GetProperty("PasswordResetToken")!.SetValue(user, command.Token);
        typeof(User).GetProperty("PasswordResetTokenExpiresAtUtc")!.SetValue(user, DateTime.UtcNow.AddHours(1));

        _userRepositoryMock
            .GetByPasswordResetTokenAsync(command.Token, Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasherMock.Hash(command.Password).Returns("hashed_password_123");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _passwordHasherMock.Received(1).Hash(command.Password);
        await _userRepositoryMock.Received(1).UpdateAsync(Arg.Is<User>(u => u != null && u.PasswordHash == "hashed_password_123"), Arg.Any<CancellationToken>());
    }
}