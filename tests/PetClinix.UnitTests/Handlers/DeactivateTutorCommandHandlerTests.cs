using FluentAssertions;
using NSubstitute;
using PetClinix.Modules.Pets.Application.Contracts;
using PetClinix.Modules.Pets.Application.UseCases.DeactivateTutor;
using PetClinix.Modules.Pets.Domain.Entities;
using PetClinix.Modules.Pets.Domain.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PetClinix.UnitTests.Handlers;

public class DeactivateTutorCommandHandlerTests
{
    private readonly ITutorRepository _tutorRepositoryMock;
    private readonly IPetsUnitOfWork _unitOfWorkMock;
    private readonly DeactivateTutorCommandHandler _handler;

    public DeactivateTutorCommandHandlerTests()
    {
        _tutorRepositoryMock = Substitute.For<ITutorRepository>();
        _unitOfWorkMock = Substitute.For<IPetsUnitOfWork>();
        _handler = new DeactivateTutorCommandHandler(_tutorRepositoryMock, _unitOfWorkMock);
    }

    private static Tutor CreateValidTutor(Guid clinicId)
    {
        return Tutor.Create(
            clinicId, Guid.NewGuid(), "Tutor Teste", "12345678900", null, "11999990000", null,
            "01001000", "Rua Teste", "123", "Centro", null, "Sao Paulo", "SP", null
        );
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_Tutor_Does_Not_Exist()
    {
        var command = new DeactivateTutorCommand(Guid.NewGuid(), Guid.NewGuid());
        _tutorRepositoryMock.GetByIdAsync(command.TutorId, Arg.Any<CancellationToken>()).Returns((Tutor?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("pets.tutor.not_found");
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_Tutor_Belongs_To_Another_Clinic()
    {
        var myClinicId = Guid.NewGuid();
        var otherClinicId = Guid.NewGuid();
        var tutor = CreateValidTutor(otherClinicId);
        var command = new DeactivateTutorCommand(myClinicId, tutor.Id);

        _tutorRepositoryMock.GetByIdAsync(command.TutorId, Arg.Any<CancellationToken>()).Returns(tutor);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("pets.tutor.not_found");
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_And_Deactivate_Tutor_When_Valid()
    {
        var clinicId = Guid.NewGuid();
        var tutor = CreateValidTutor(clinicId);
        var command = new DeactivateTutorCommand(clinicId, tutor.Id);

        _tutorRepositoryMock.GetByIdAsync(command.TutorId, Arg.Any<CancellationToken>()).Returns(tutor);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tutor.IsActive.Should().BeFalse();

        await _tutorRepositoryMock.Received(1).UpdateAsync(Arg.Any<Tutor>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}