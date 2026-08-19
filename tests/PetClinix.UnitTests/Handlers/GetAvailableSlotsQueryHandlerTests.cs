using FluentAssertions;
using NSubstitute;
using PetClinix.Modules.Appointments.Application.Contracts;
using PetClinix.Modules.Appointments.Application.UseCases.GetAvailableSlots;
using PetClinix.Modules.Appointments.Domain.Entities;
using PetClinix.Modules.Appointments.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PetClinix.UnitTests.Handlers;

public class GetAvailableSlotsQueryHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepositoryMock;
    private readonly IServiceCatalogService _serviceCatalogServiceMock;
    private readonly IClinicScheduleService _clinicScheduleServiceMock;
    private readonly GetAvailableSlotsQueryHandler _handler;

    public GetAvailableSlotsQueryHandlerTests()
    {
        _appointmentRepositoryMock = Substitute.For<IAppointmentRepository>();
        _serviceCatalogServiceMock = Substitute.For<IServiceCatalogService>();
        _clinicScheduleServiceMock = Substitute.For<IClinicScheduleService>();
        _handler = new GetAvailableSlotsQueryHandler(_appointmentRepositoryMock, _serviceCatalogServiceMock, _clinicScheduleServiceMock);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Slots_When_No_Appointments_Exist()
    {
        var date = new DateOnly(2026, 8, 15);
        var query = new GetAvailableSlotsQuery(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), date);

        _clinicScheduleServiceMock.GetWorkingHours().Returns((new TimeOnly(8, 0), new TimeOnly(10, 0)));
        _serviceCatalogServiceMock.GetDurationInMinutesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(30);
        _appointmentRepositoryMock.GetByVeterinarianAndDateAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Appointment>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(new List<string> { "08:00", "08:30", "09:00", "09:30" });
    }

    [Fact]
    public async Task Handle_Should_Skip_Conflicting_Slot_When_Appointment_Exists()
    {
        var tomorrow = DateTime.UtcNow.AddDays(1);
        var date = DateOnly.FromDateTime(tomorrow);
        var query = new GetAvailableSlotsQuery(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), date);

        _clinicScheduleServiceMock.GetWorkingHours().Returns((new TimeOnly(8, 0), new TimeOnly(10, 0)));
        _serviceCatalogServiceMock.GetDurationInMinutesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(30);

        var conflictingDate = new DateTime(date.Year, date.Month, date.Day, 8, 30, 0, DateTimeKind.Utc);
        var existingAppt = Appointment.Create(
            query.ClinicId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), query.VeterinarianId,
            conflictingDate, null, Guid.NewGuid());

        _appointmentRepositoryMock.GetByVeterinarianAndDateAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Appointment> { existingAppt });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(new List<string> { "08:00", "09:00", "09:30" });
        result.Value.Should().NotContain("08:30");
    }
}