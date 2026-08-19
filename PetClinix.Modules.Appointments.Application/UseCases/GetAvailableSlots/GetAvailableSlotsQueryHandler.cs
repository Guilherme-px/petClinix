using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Appointments.Application.Contracts;
using PetClinix.Modules.Appointments.Domain.Repositories;

namespace PetClinix.Modules.Appointments.Application.UseCases.GetAvailableSlots;

public sealed class GetAvailableSlotsQueryHandler : ICommandHandler<GetAvailableSlotsQuery, Result<List<string>>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IServiceCatalogService _serviceCatalogService;
    private readonly IClinicScheduleService _clinicScheduleService;

    public GetAvailableSlotsQueryHandler(
        IAppointmentRepository appointmentRepository,
        IServiceCatalogService serviceCatalogService,
        IClinicScheduleService clinicScheduleService)
    {
        _appointmentRepository = appointmentRepository;
        _serviceCatalogService = serviceCatalogService;
        _clinicScheduleService = clinicScheduleService;
    }

    public async Task<Result<List<string>>> Handle(GetAvailableSlotsQuery query, CancellationToken cancellationToken)
    {
        var durationMinutes = await _serviceCatalogService.GetDurationInMinutesAsync(query.ServiceId, cancellationToken);
        if (durationMinutes <= 0) return Result<List<string>>.Failure("appointments.slots.invalid_duration", "Duração do serviço inválida.");

        var (startTime, endTime) = _clinicScheduleService.GetWorkingHours();
        var dateUtc = query.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var existingAppointments = await _appointmentRepository.GetByVeterinarianAndDateAsync(query.VeterinarianId, dateUtc, cancellationToken);

        var busySlots = existingAppointments
            .Select(a => new { Start = a.ScheduledDateUtc, End = a.ScheduledDateUtc.AddMinutes(durationMinutes) })
            .ToList();

        var availableSlots = new List<string>();
        var slotStart = query.Date.ToDateTime(startTime, DateTimeKind.Utc);

        while (slotStart.TimeOfDay < endTime.ToTimeSpan())
        {
            var slotEnd = slotStart.AddMinutes(durationMinutes);

            if (slotEnd.TimeOfDay > endTime.ToTimeSpan() && slotEnd.Date == slotStart.Date)
                break;

            bool hasConflict = busySlots.Any(busy =>
                slotStart < busy.End && slotEnd > busy.Start);

            if (!hasConflict)
            {
                availableSlots.Add(slotStart.ToString("HH:mm"));
            }

            slotStart = slotEnd;
        }

        return Result<List<string>>.Success(availableSlots);
    }
}