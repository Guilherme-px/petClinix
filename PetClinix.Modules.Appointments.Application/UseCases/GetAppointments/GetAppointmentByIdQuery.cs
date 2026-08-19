using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Appointments.Application.UseCases.GetAppointments;

public sealed record GetAppointmentByIdQuery(Guid ClinicId, Guid AppointmentId) : ICommand<Result<AppointmentResponse>>;