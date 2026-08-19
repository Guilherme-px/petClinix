using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Appointments.Domain.Enums;

namespace PetClinix.Modules.Appointments.Application.UseCases.UpdateAppointmentStatus;

public sealed record UpdateAppointmentStatusCommand(
    Guid ClinicId, Guid AppointmentId, Guid UpdatedByUserId, AppointmentStatus NewStatus
) : ICommand<Result>;