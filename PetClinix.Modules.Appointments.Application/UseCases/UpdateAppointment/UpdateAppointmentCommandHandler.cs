using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Appointments.Application.Contracts;
using PetClinix.Modules.Appointments.Domain.Exceptions;
using PetClinix.Modules.Appointments.Domain.Repositories;

namespace PetClinix.Modules.Appointments.Application.UseCases.UpdateAppointment;

public sealed class UpdateAppointmentCommandHandler : ICommandHandler<UpdateAppointmentCommand, Result>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentsUnitOfWork _unitOfWork;

    public UpdateAppointmentCommandHandler(IAppointmentRepository appointmentRepository, IAppointmentsUnitOfWork unitOfWork)
    {
        _appointmentRepository = appointmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateAppointmentCommand command, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(command.AppointmentId, cancellationToken);

        if (appointment == null || appointment.ClinicId != command.ClinicId)
        {
            return Result.Failure("appointments.appt.not_found", "Agendamento não encontrado.");
        }

        var normalizedDate = command.ScheduledDateUtc.AddTicks(-(command.ScheduledDateUtc.Ticks % TimeSpan.TicksPerMinute));
        var existingAppointments = await _appointmentRepository.GetByVeterinarianAndDateAsync(command.VeterinarianId, normalizedDate, cancellationToken);

        var hasConflict = existingAppointments.Any(a =>
            a.Id != command.AppointmentId &&
            a.ScheduledDateUtc.Year == normalizedDate.Year &&
            a.ScheduledDateUtc.Month == normalizedDate.Month &&
            a.ScheduledDateUtc.Day == normalizedDate.Day &&
            a.ScheduledDateUtc.Hour == normalizedDate.Hour &&
            a.ScheduledDateUtc.Minute == normalizedDate.Minute
        );

        if (hasConflict)
        {
            return Result.Failure("appointments.appt.slot_taken", "O veterinário já possui um agendamento neste exato horário.");
        }

        try
        {
            appointment.UpdateDetails(
                command.UpdatedByUserId,
                command.VeterinarianId,
                command.ServiceId,
                normalizedDate,
                command.Notes
            );

            await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (AppointmentsDomainException ex)
        {
            return Result.Failure(ex.Code, ex.Message);
        }
    }
}