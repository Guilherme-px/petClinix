using FluentAssertions;
using NSubstitute;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Application.UseCases.UpdateClinic;
using PetClinix.Modules.Identity.Domain.Entities;
using PetClinix.Modules.Identity.Domain.Repositories;
using PetClinix.Modules.Identity.Domain.ValueObjects;
using System;
using Xunit;

namespace PetClinix.UnitTests.Handlers;

public class UpdateClinicCommandHandlerTests
{
    private readonly IClinicRepository _clinicRepositoryMock;
    private readonly UpdateClinicCommandHandler _handler;
    private readonly IUnitOfWork _unitOfWorkMock;

    public UpdateClinicCommandHandlerTests()
    {
        _clinicRepositoryMock = Substitute.For<IClinicRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _handler = new UpdateClinicCommandHandler(_clinicRepositoryMock, _unitOfWorkMock);
    }

    private static Clinic CreateValidClinic()
    {
        return Clinic.Create(
            "Clinica Teste", "Teste LTDA", "12345678000199",
            ClinicSlug.Create("clinica-teste"),
            "clinica@teste.com", "11988887777",
            "01001000", "Rua Teste", "123", "Centro",
            null, "SP", "SP");
    }

    private static UpdateClinicCommand CreateValidCommand(Guid clinicId) => new(
       clinicId,
            "Novo Nome", "Nova Razao", "98765432000111",
            "novo@email.com", "1188887777",
            "01001000", "Nova Rua", "999", "Novo Bairro",
            null, "Santos", "SP"
    );

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_Clinic_Does_Not_Exist()
    {
        var command = CreateValidCommand(Guid.NewGuid());
        _clinicRepositoryMock.GetByIdAsync(command.ClinicId, Arg.Any<CancellationToken>()).Returns((Clinic?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("identity.clinic.not_found");
        await _clinicRepositoryMock.DidNotReceive().UpdateAsync(Arg.Any<Clinic>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_Domain_Throws_Exception()
    {
        var clinic = CreateValidClinic();
        var command = CreateValidCommand(clinic.Id) with { TradeName = "" };

        _clinicRepositoryMock.GetByIdAsync(command.ClinicId, Arg.Any<CancellationToken>()).Returns(clinic);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("identity.clinic.trade_name_required");
        await _clinicRepositoryMock.DidNotReceive().UpdateAsync(Arg.Any<Clinic>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_And_Update_Clinic_When_Valid()
    {
        var clinic = CreateValidClinic();
        var command = CreateValidCommand(clinic.Id);

        _clinicRepositoryMock.GetByIdAsync(command.ClinicId, Arg.Any<CancellationToken>()).Returns(clinic);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        clinic.TradeName.Should().Be(command.TradeName);
        clinic.Email.Value.Should().Be(command.Email);
        await _clinicRepositoryMock.Received(1).UpdateAsync(Arg.Any<Clinic>(), Arg.Any<CancellationToken>());
    }
}