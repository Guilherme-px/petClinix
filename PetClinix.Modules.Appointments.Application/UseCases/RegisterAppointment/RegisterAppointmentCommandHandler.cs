using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Appointments.Application.Contracts;
using PetClinix.Modules.Appointments.Domain.Entities;
using PetClinix.Modules.Appointments.Domain.Exceptions;
using PetClinix.Modules.Appointments.Domain.Repositories;

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
        var existingAppointments = await _appointmentRepository.GetByVeterinarianAndDateAsync(command.VeterinarianId, command.ScheduledDateUtc, cancellationToken);

        if (existingAppointments.Any(a => a.ScheduledDateUtc == command.ScheduledDateUtc))
        {
            return Result.Failure("appointments.appt.slot_taken", "O veterinário já possui um agendamento neste exato horário.");
        }

        try
        {
            var appointment = Appointment.Create(
                command.ClinicId, command.TutorId, command.PetId, command.ServiceId, command.VeterinarianId,
                command.ScheduledDateUtc, command.Notes, command.CreatedByUserId
            );

            await _appointmentRepository.AddAsync(appointment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (AppointmentsDomainException ex)
        {
            return Result.Failure(ex.Code, ex.Message);
        }
    }
}