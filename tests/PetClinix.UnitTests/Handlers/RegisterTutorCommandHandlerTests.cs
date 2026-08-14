using FluentAssertions;
using NSubstitute;
using PetClinix.Modules.Pets.Application.Contracts;
using PetClinix.Modules.Pets.Application.UseCases.RegisterTutor;
using PetClinix.Modules.Pets.Domain.Entities;
using PetClinix.Modules.Pets.Domain.Repositories;
using PetClinix.Modules.Pets.Domain.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PetClinix.UnitTests.Handlers;

public class RegisterTutorCommandHandlerTests
{
    private readonly ITutorRepository _tutorRepositoryMock;
    private readonly IPetsUnitOfWork _unitOfWorkMock;
    private readonly RegisterTutorCommandHandler _handler;

    public RegisterTutorCommandHandlerTests()
    {
        _tutorRepositoryMock = Substitute.For<ITutorRepository>();
        _unitOfWorkMock = Substitute.For<IPetsUnitOfWork>();
        _handler = new RegisterTutorCommandHandler(_tutorRepositoryMock, _unitOfWorkMock);
    }

    private static RegisterTutorCommand CreateValidCommand() => new(
        Guid.NewGuid(),
        Guid.NewGuid(),
        "João da Silva",
        "12345678900",
        null,
        "11988887777",
        null,
        "01001000",
        "Rua Teste",
        "123",
        "Centro",
        null,
        "Sao Paulo",
        "SP",
        null
    );

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_Cpf_Already_Exists()
    {
        var command = CreateValidCommand();

        _tutorRepositoryMock.ExistsByCpfAsync(command.ClinicId, Arg.Any<Cpf>(), Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("pets.tutor.cpf_already_exists");
        await _tutorRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Tutor>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_Domain_Throws_Exception()
    {
        var command = CreateValidCommand() with { Name = "" };

        _tutorRepositoryMock.ExistsByCpfAsync(Arg.Any<Guid>(), Arg.Any<Cpf>(), Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("pets.tutor.name_required");
        await _tutorRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Tutor>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_And_Save_Tutor_When_Valid()
    {
        var command = CreateValidCommand();
        _tutorRepositoryMock.ExistsByCpfAsync(Arg.Any<Guid>(), Arg.Any<Cpf>(), Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _tutorRepositoryMock.Received(1).AddAsync(Arg.Any<Tutor>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}