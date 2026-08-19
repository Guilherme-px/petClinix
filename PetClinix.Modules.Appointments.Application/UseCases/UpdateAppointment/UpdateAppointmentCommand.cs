using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Appointments.Application.UseCases.UpdateAppointment;

public sealed record UpdateAppointmentCommand(
    Guid ClinicId, Guid AppointmentId, Guid UpdatedByUserId,
    Guid VeterinarianId, Guid ServiceId, DateTime ScheduledDateUtc, string? Notes
) : ICommand<Result>;