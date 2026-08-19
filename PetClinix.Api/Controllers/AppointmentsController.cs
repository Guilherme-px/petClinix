using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Appointments.Application.UseCases.RegisterAppointment;
using PetClinix.Modules.Appointments.Application.UseCases.GetAvailableSlots;
using PetClinix.Modules.Appointments.Application.UseCases.GetAppointments;
using System.Security.Claims;

namespace PetClinix.Api.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly ICommandHandler<RegisterAppointmentCommand, Result> _registerAppointmentHandler;
    private readonly ICommandHandler<GetAvailableSlotsQuery, Result<List<string>>> _getSlotsHandler;
    private readonly ICommandHandler<GetAppointmentsQuery, Result<PagedResult<AppointmentResponse>>> _getAppointmentsHandler;

    public AppointmentsController(
        ICommandHandler<RegisterAppointmentCommand, Result> registerAppointmentHandler,
        ICommandHandler<GetAvailableSlotsQuery, Result<List<string>>> getSlotsHandler,
        ICommandHandler<GetAppointmentsQuery, Result<PagedResult<AppointmentResponse>>> getAppointmentsHandler)
    {
        _registerAppointmentHandler = registerAppointmentHandler;
        _getSlotsHandler = getSlotsHandler;
        _getAppointmentsHandler = getAppointmentsHandler;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterAppointment([FromBody] RegisterAppointmentRequest request, CancellationToken cancellationToken)
    {
        var clinicIdClaim = User.FindFirst("clinic_id")?.Value;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(clinicIdClaim, out var clinicId) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Token inválido." });
        }

        var command = new RegisterAppointmentCommand(
            clinicId, request.TutorId, request.PetId, request.ServiceId, request.VeterinarianId,
            request.ScheduledDateUtc, request.Notes, userId
        );

        var result = await _registerAppointmentHandler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { result.ErrorCode, result.ErrorMessage });
        }

        return NoContent();
    }

    [HttpGet("available-slots")]
    public async Task<IActionResult> GetAvailableSlots([FromQuery] Guid vetId, [FromQuery] Guid serviceId, [FromQuery] DateOnly date, CancellationToken cancellationToken)
    {
        var clinicIdClaim = User.FindFirst("clinic_id")?.Value;
        if (!Guid.TryParse(clinicIdClaim, out var clinicId))
        {
            return Unauthorized(new { message = "Token inválido." });
        }

        var query = new GetAvailableSlotsQuery(clinicId, vetId, serviceId, date);
        var result = await _getSlotsHandler.Handle(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { result.ErrorCode, result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAppointments([FromQuery] DateTime date, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var clinicIdClaim = User.FindFirst("clinic_id")?.Value;
        
        if (!Guid.TryParse(clinicIdClaim, out var clinicId))
        {
            return Unauthorized(new { message = "Token inválido ou sem ID da clínica." });
        }

        var query = new GetAppointmentsQuery(clinicId, date, pageNumber, pageSize);
        var result = await _getAppointmentsHandler.Handle(query, cancellationToken);

        return Ok(result.Value);
    }
}

public record RegisterAppointmentRequest(
    Guid TutorId, Guid PetId, Guid ServiceId, Guid VeterinarianId,
    DateTime ScheduledDateUtc, string? Notes
);