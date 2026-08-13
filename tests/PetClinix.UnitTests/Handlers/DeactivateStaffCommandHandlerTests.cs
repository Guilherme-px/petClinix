using FluentAssertions;
using NSubstitute;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Application.UseCases.DeactivateStaff;
using PetClinix.Modules.Identity.Domain.Entities;
using PetClinix.Modules.Identity.Domain.Enums;
using PetClinix.Modules.Identity.Domain.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PetClinix.UnitTests.Handlers;

public class DeactivateStaffCommandHandlerTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly DeactivateStaffCommandHandler _handler;

    public DeactivateStaffCommandHandlerTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _handler = new DeactivateStaffCommandHandler(_userRepositoryMock, _unitOfWorkMock);
    }

    private static User CreateValidStaffUser(Guid clinicId)
    {
        return User.CreateStaff(
            clinicId, "Dr. Dolittle", "dr@teste.com", "hash", "12345678900", "11999990000",
            new DateOnly(1990, 1, 1), UserRole.Veterinarian
        );
    }

    private static DeactivateStaffCommand CreateValidCommand(Guid clinicId, Guid userId) =>
       new(clinicId, userId);

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_User_Does_Not_Exist()
    {
        var command = CreateValidCommand(Guid.NewGuid(), Guid.NewGuid());
        _userRepositoryMock.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("identity.user.not_found");
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_User_Belongs_To_Another_Clinic()
    {
        var myClinicId = Guid.NewGuid();
        var otherClinicId = Guid.NewGuid();
        var user = CreateValidStaffUser(otherClinicId);

        var command = CreateValidCommand(myClinicId, user.Id);
        _userRepositoryMock.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("identity.user.not_found");
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]

    public async Task Handle_Should_ReturnFailure_When_User_Is_Admin()
    {
        var clinicId = Guid.NewGuid();
        var adminUser = User.CreateAdmin(
            clinicId, "Admin", "admin@teste.com", "hash", "12345678900",
            "11999990000", new DateOnly(1990, 1, 1));

        var command = CreateValidCommand(clinicId, adminUser.Id);
        _userRepositoryMock.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>()).Returns(adminUser);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("identity.user.cannot_deactivate_admin");
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_And_Deactivate_User_When_Valid()
    {
        var clinicId = Guid.NewGuid();
        var user = CreateValidStaffUser(clinicId);
        var command = CreateValidCommand(clinicId, user.Id);

        _userRepositoryMock.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeFalse();

        await _userRepositoryMock.Received(1).UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}