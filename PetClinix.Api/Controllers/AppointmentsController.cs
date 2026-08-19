using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Appointments.Application.UseCases.RegisterAppointment;
using System.Security.Claims;

namespace PetClinix.Api.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly ICommandHandler<RegisterAppointmentCommand, Result> _registerAppointmentHandler;

    public AppointmentsController(ICommandHandler<RegisterAppointmentCommand, Result> registerAppointmentHandler)
    {
        _registerAppointmentHandler = registerAppointmentHandler;
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
}

public record RegisterAppointmentRequest(
    Guid TutorId, Guid PetId, Guid ServiceId, Guid VeterinarianId,
    DateTime ScheduledDateUtc, string? Notes
);