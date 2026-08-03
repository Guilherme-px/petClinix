using FluentAssertions;
using NSubstitute;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Application.UseCases.UpdateAccount;
using PetClinix.Modules.Identity.Domain.Entities;
using PetClinix.Modules.Identity.Domain.Repositories;
using PetClinix.Modules.Identity.Domain.ValueObjects;
using System;
using Xunit;

namespace PetClinix.UnitTests.Handlers;

public class UpdateAccountCommandHandlerTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IClinicRepository _clinicRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly UpdateAccountCommandHandler _handler;

    public UpdateAccountCommandHandlerTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _clinicRepositoryMock = Substitute.For<IClinicRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _handler = new UpdateAccountCommandHandler(_userRepositoryMock, _clinicRepositoryMock, _unitOfWorkMock);
    }

    private static User CreateValidUser() =>
        User.CreateAdmin(Guid.NewGuid(), "Admin", "admin@test.com", "hash", "12345678900", "11999990000", new DateOnly(1990, 1, 1));

    private static Clinic CreateValidClinic(Guid clinicId) =>
        Clinic.Create("Clinica", "LTDA", "12345678000199", ClinicSlug.Create("clinica"), "c@t.com", "11988887777", "01001000", "Rua", "123", "Centro", null, "SP", "SP");

    private static UpdateAccountCommand CreateValidCommand(Guid userId, Guid clinicId) => new(
        userId, clinicId,
        "Novo Nome", "1188887777", new DateOnly(1991, 5, 10),
        "Nova Clinica", "Nova Razao", "98765432000111", "novo@c.com", "1177778888",
        "01001000", "Nova Rua", "999", "Novo Bairro", null, "Santos", "SP");

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
    public async Task Handle_Should_ReturnFailure_When_Clinic_Does_Not_Exist()
    {
        var user = CreateValidUser();
        var command = CreateValidCommand(user.Id, Guid.NewGuid());

        _userRepositoryMock.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>()).Returns(user);
        _clinicRepositoryMock.GetByIdAsync(command.ClinicId, Arg.Any<CancellationToken>()).Returns((Clinic?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("identity.clinic.not_found");
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_And_Call_SaveChanges_When_Valid()
    {
        var user = CreateValidUser();
        var clinic = CreateValidClinic(user.ClinicId);
        var command = CreateValidCommand(user.Id, clinic.Id);

        _userRepositoryMock.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>()).Returns(user);
        _clinicRepositoryMock.GetByIdAsync(command.ClinicId, Arg.Any<CancellationToken>()).Returns(clinic);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        await _userRepositoryMock.Received(1).UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _clinicRepositoryMock.Received(1).UpdateAsync(Arg.Any<Clinic>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}