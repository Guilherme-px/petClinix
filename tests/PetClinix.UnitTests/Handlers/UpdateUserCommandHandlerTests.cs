using FluentAssertions;
using NSubstitute;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Application.UseCases.UpdateProfile;
using PetClinix.Modules.Identity.Domain.Entities;
using PetClinix.Modules.Identity.Domain.Repositories;
using System;
using Xunit;

namespace PetClinix.UnitTests.Handlers;

public class UpdateUserCommandHandlerTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly UpdateUserCommandHandler _handler;
    private readonly IUnitOfWork _unitOfWorkMock;   

    public UpdateUserCommandHandlerTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _handler = new UpdateUserCommandHandler(_userRepositoryMock, _unitOfWorkMock);
    }

    private static User CreateValidUser()
    {
        return User.CreateAdmin(
            Guid.NewGuid(), "Admin", "admin@test.com", "hash",
            "12345678900", "11999990000", new DateOnly(1990, 1, 1));
    }

    private static UpdateUserCommand CreateValidCommand(Guid userId) => new(
        userId, "Novo Nome", "1188887777", new DateOnly(1991, 5, 10));

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_User_Does_Not_Exist()
    {
        var command = CreateValidCommand(Guid.NewGuid());
        _userRepositoryMock.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("identity.user.not_found");
        await _userRepositoryMock.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_Domain_Throws_Exception()
    {
        var user = CreateValidUser();
        var command = new UpdateUserCommand(user.Id, "", "1188887777", new DateOnly(1991, 5, 10));

        _userRepositoryMock.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("identity.user.name_required");
        await _userRepositoryMock.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_And_Update_User_When_Valid()
    {
        var user = CreateValidUser();
        var command = CreateValidCommand(user.Id);

        _userRepositoryMock.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.Name.Should().Be(command.Name);
        await _userRepositoryMock.Received(1).UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }
}