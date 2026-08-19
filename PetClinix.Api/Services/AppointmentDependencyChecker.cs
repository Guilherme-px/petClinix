using Microsoft.EntityFrameworkCore;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Appointments.Domain.Enums;
using PetClinix.Modules.Appointments.Infrastructure.Persistence;

namespace PetClinix.Api.Services;

public class AppointmentDependencyChecker : IAppointmentDependencyChecker
{
    private readonly AppointmentsDbContext _context;

    public AppointmentDependencyChecker(AppointmentsDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasFutureAppointmentsForTutorAsync(Guid tutorId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Appointments.AnyAsync(a =>
            a.TutorId == tutorId && a.ScheduledDateUtc >= now &&
            (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed),
            cancellationToken
        );
    }

    public async Task<bool> HasFutureAppointmentsForPetAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Appointments.AnyAsync(a =>
            a.PetId == petId && a.ScheduledDateUtc >= now &&
            (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed),
            cancellationToken
        );
    }

    public async Task<bool> HasFutureAppointmentsForVetAsync(Guid vetId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Appointments.AnyAsync(a =>
            a.VeterinarianId == vetId && a.ScheduledDateUtc >= now &&
            (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed),
            cancellationToken
        );
    }

    public async Task<bool> HasFutureAppointmentsForServiceAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Appointments.AnyAsync(a =>
            a.ServiceId == serviceId && a.ScheduledDateUtc >= now &&
            (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed),
            cancellationToken
        );
    }
}