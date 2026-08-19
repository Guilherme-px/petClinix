using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Appointments.Application.UseCases.RegisterAppointment;
using PetClinix.Modules.Appointments.Application.UseCases.GetAvailableSlots;
using System.Security.Claims;

namespace PetClinix.Api.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly ICommandHandler<RegisterAppointmentCommand, Result> _registerAppointmentHandler;
    private readonly ICommandHandler<GetAvailableSlotsQuery, Result<List<string>>> _getSlotsHandler;

    public AppointmentsController(
        ICommandHandler<RegisterAppointmentCommand, Result> registerAppointmentHandler,
        ICommandHandler<GetAvailableSlotsQuery, Result<List<string>>> getSlotsHandler)
    {
        _registerAppointmentHandler = registerAppointmentHandler;
        _getSlotsHandler = getSlotsHandler;
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
}

public record RegisterAppointmentRequest(
    Guid TutorId, Guid PetId, Guid ServiceId, Guid VeterinarianId,
    DateTime ScheduledDateUtc, string? Notes
);