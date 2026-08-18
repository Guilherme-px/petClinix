using FluentAssertions;
using NSubstitute;
using PetClinix.Modules.Catalog.Application.Contracts;
using PetClinix.Modules.Catalog.Application.UseCases.RegisterService;
using PetClinix.Modules.Catalog.Domain.Entities;
using PetClinix.Modules.Catalog.Domain.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PetClinix.UnitTests.Handlers;

public class RegisterServiceCommandHandlerTests
{
    private readonly IServiceRepository _serviceRepositoryMock;
    private readonly ICatalogUnitOfWork _unitOfWorkMock;
    private readonly RegisterServiceCommandHandler _handler;

    public RegisterServiceCommandHandlerTests()
    {
        _serviceRepositoryMock = Substitute.For<IServiceRepository>();
        _unitOfWorkMock = Substitute.For<ICatalogUnitOfWork>();
        _handler = new RegisterServiceCommandHandler(_serviceRepositoryMock, _unitOfWorkMock);
    }

    private static RegisterServiceCommand CreateValidCommand() => new(
        Guid.NewGuid(),
        Guid.NewGuid(),
        "Consulta Clínica Geral",
        "Consulta padrão",
        30,
        150.00m,
        true
    );

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_Name_Already_Exists()
    {
        var command = CreateValidCommand();

        _serviceRepositoryMock.ExistsByNameAsync(command.ClinicId, command.Name, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("catalog.service.name_already_exists");
        await _serviceRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Service>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_Domain_Throws_Exception()
    {
        var command = CreateValidCommand() with { DurationInMinutes = 0 };

        _serviceRepositoryMock.ExistsByNameAsync(command.ClinicId, command.Name, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("catalog.service.invalid_duration");
        await _serviceRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Service>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_And_Save_Service_When_Valid()
    {
        var command = CreateValidCommand();

        _serviceRepositoryMock.ExistsByNameAsync(command.ClinicId, command.Name, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _serviceRepositoryMock.Received(1).AddAsync(Arg.Any<Service>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}