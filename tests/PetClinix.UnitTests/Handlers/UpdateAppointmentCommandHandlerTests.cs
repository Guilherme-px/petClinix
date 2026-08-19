using FluentAssertions;
using NSubstitute;
using PetClinix.Modules.Appointments.Application.Contracts;
using PetClinix.Modules.Appointments.Application.UseCases.UpdateAppointment;
using PetClinix.Modules.Appointments.Domain.Entities;
using PetClinix.Modules.Appointments.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PetClinix.UnitTests.Handlers;

public class UpdateAppointmentCommandHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepositoryMock;
    private readonly IAppointmentsUnitOfWork _unitOfWorkMock;
    private readonly UpdateAppointmentCommandHandler _handler;

    public UpdateAppointmentCommandHandlerTests()
    {
        _appointmentRepositoryMock = Substitute.For<IAppointmentRepository>();
        _unitOfWorkMock = Substitute.For<IAppointmentsUnitOfWork>();
        _handler = new UpdateAppointmentCommandHandler(_appointmentRepositoryMock, _unitOfWorkMock);
    }

    private static Appointment CreateValidAppointment(Guid clinicId)
    {
        return Appointment.Create(
            clinicId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1), "Original", Guid.NewGuid()
        );
    }

    private static UpdateAppointmentCommand CreateValidCommand(Guid clinicId, Guid apptId, Guid vetId) => new(
        clinicId, apptId, Guid.NewGuid(), vetId, Guid.NewGuid(),
        DateTime.UtcNow.AddDays(2), "Notas atualizadas"
    );

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_Appointment_Does_Not_Exist()
    {
        var command = CreateValidCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        _appointmentRepositoryMock.GetByIdAsync(command.AppointmentId, Arg.Any<CancellationToken>()).Returns((Appointment?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("appointments.appt.not_found");
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_Double_Booking()
    {
        var clinicId = Guid.NewGuid();
        var vetId = Guid.NewGuid();
        var appt = CreateValidAppointment(clinicId);
        appt.GetType().GetProperty("VeterinarianId")!.SetValue(appt, vetId);

        var command = CreateValidCommand(clinicId, appt.Id, vetId);

        var conflictingAppt = CreateValidAppointment(clinicId);
        conflictingAppt.GetType().GetProperty("VeterinarianId")!.SetValue(conflictingAppt, vetId);
        conflictingAppt.GetType().GetProperty("ScheduledDateUtc")!.SetValue(conflictingAppt, command.ScheduledDateUtc);

        _appointmentRepositoryMock.GetByIdAsync(command.AppointmentId, Arg.Any<CancellationToken>()).Returns(appt);
        _appointmentRepositoryMock.GetByVeterinarianAndDateAsync(vetId, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Appointment> { conflictingAppt });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("appointments.appt.slot_taken");
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_Appointment_Is_Completed()
    {
        var clinicId = Guid.NewGuid();
        var vetId = Guid.NewGuid();
        var appt = CreateValidAppointment(clinicId);
        appt.GetType().GetProperty("VeterinarianId")!.SetValue(appt, vetId);
        appt.Complete(Guid.NewGuid());

        var command = CreateValidCommand(clinicId, appt.Id, vetId);

        _appointmentRepositoryMock.GetByIdAsync(command.AppointmentId, Arg.Any<CancellationToken>()).Returns(appt);
        _appointmentRepositoryMock.GetByVeterinarianAndDateAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Appointment>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("appointments.appt.invalid_status");
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_And_Update_Appointment_When_Valid()
    {
        var clinicId = Guid.NewGuid();
        var vetId = Guid.NewGuid();
        var appt = CreateValidAppointment(clinicId);
        appt.GetType().GetProperty("VeterinarianId")!.SetValue(appt, vetId);

        var command = CreateValidCommand(clinicId, appt.Id, vetId);

        _appointmentRepositoryMock.GetByIdAsync(command.AppointmentId, Arg.Any<CancellationToken>()).Returns(appt);
        _appointmentRepositoryMock.GetByVeterinarianAndDateAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Appointment>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        appt.Notes.Should().Be(command.Notes);
        var normalizedDate = command.ScheduledDateUtc.AddTicks(-(command.ScheduledDateUtc.Ticks % TimeSpan.TicksPerMinute));
        appt.ScheduledDateUtc.Should().Be(normalizedDate);

        await _appointmentRepositoryMock.Received(1).UpdateAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}