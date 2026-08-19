using PetClinix.Modules.Appointments.Domain.Enums;

namespace PetClinix.Modules.Appointments.Application.UseCases.GetAppointments;

public sealed record AppointmentResponse(
    Guid Id,
    Guid TutorId,
    Guid PetId,
    Guid ServiceId,
    Guid VeterinarianId,
    DateTime ScheduledDateUtc,
    string? Notes,
    AppointmentStatus Status
);