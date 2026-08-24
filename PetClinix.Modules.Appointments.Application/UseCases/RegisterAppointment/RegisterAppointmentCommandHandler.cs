using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Appointments.Application.Contracts;
using PetClinix.Modules.Appointments.Domain.Entities;
using PetClinix.Modules.Appointments.Domain.Exceptions;
using PetClinix.Modules.Appointments.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace PetClinix.Modules.Appointments.Application.UseCases.RegisterAppointment;

public sealed class RegisterAppointmentCommandHandler : ICommandHandler<RegisterAppointmentCommand, Result>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentsUnitOfWork _unitOfWork;

    public RegisterAppointmentCommandHandler(IAppointmentRepository appointmentRepository, IAppointmentsUnitOfWork unitOfWork)
    {
        _appointmentRepository = appointmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RegisterAppointmentCommand command, CancellationToken cancellationToken)
    {
        var normalizedDate = command.ScheduledDateUtc.AddTicks(-(command.ScheduledDateUtc.Ticks % TimeSpan.TicksPerMinute));
        var existingAppointments = await _appointmentRepository.GetByVeterinarianAndDateAsync(command.VeterinarianId, normalizedDate, cancellationToken);

        var hasConflict = existingAppointments.Any(a =>
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
            var appointment = Appointment.Create(
                command.ClinicId, command.TutorId, command.PetId, command.ServiceId, command.VeterinarianId,
                normalizedDate, command.Notes, command.CreatedByUserId
            );
            
            await _appointmentRepository.AddAsync(appointment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure("appointments.appt.slot_taken", "Ocorreu um conflito de horários e o agendamento não pôde ser salvo. Tente novamente.");
        }
        catch (AppointmentsDomainException ex)
        {
            return Result.Failure(ex.Code, ex.Message);
        }
    }
}