using FluentAssertions;
using NSubstitute;
using PetClinix.Modules.Appointments.Application.Contracts;
using PetClinix.Modules.Appointments.Application.UseCases.RegisterAppointment;
using PetClinix.Modules.Appointments.Domain.Entities;
using PetClinix.Modules.Appointments.Domain.Enums;
using PetClinix.Modules.Appointments.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PetClinix.UnitTests.Handlers;

public class RegisterAppointmentCommandHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepositoryMock;
    private readonly IAppointmentsUnitOfWork _unitOfWorkMock;
    private readonly RegisterAppointmentCommandHandler _handler;

    public RegisterAppointmentCommandHandlerTests()
    {
        _appointmentRepositoryMock = Substitute.For<IAppointmentRepository>();
        _unitOfWorkMock = Substitute.For<IAppointmentsUnitOfWork>();
        _handler = new RegisterAppointmentCommandHandler(_appointmentRepositoryMock, _unitOfWorkMock);
    }

    private static RegisterAppointmentCommand CreateValidCommand(DateTime? scheduledDate = null) => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
        scheduledDate ?? DateTime.UtcNow.AddDays(1), "Observações", Guid.NewGuid()
    );

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_Slot_Is_Taken()
    {
        var command = CreateValidCommand();

        var existingAppointment = Appointment.Create(
            command.ClinicId, command.TutorId, command.PetId, command.ServiceId, command.VeterinarianId,
            command.ScheduledDateUtc, null, Guid.NewGuid()
        );

        _appointmentRepositoryMock.GetByVeterinarianAndDateAsync(command.VeterinarianId, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Appointment> { existingAppointment });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("appointments.appt.slot_taken");
        await _appointmentRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_Date_Is_In_The_Past()
    {
        var command = CreateValidCommand(DateTime.UtcNow.AddDays(-1));

        _appointmentRepositoryMock.GetByVeterinarianAndDateAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Appointment>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("appointments.appt.past_date");
        await _appointmentRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_And_Save_Appointment_When_Valid()
    {
        var command = CreateValidCommand();

        _appointmentRepositoryMock.GetByVeterinarianAndDateAsync(command.VeterinarianId, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Appointment>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _appointmentRepositoryMock.Received(1).AddAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}