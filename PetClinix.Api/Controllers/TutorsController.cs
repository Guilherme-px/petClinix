using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Pets.Application.UseCases.RegisterTutor;
using System.Security.Claims;

namespace PetClinix.Api.Controllers;

[ApiController]
[Route("api/tutors")]
[Authorize]
public class TutorsController : ControllerBase
{
    private readonly ICommandHandler<RegisterTutorCommand, Result> _registerTutorHandler;

    public TutorsController(ICommandHandler<RegisterTutorCommand, Result> registerTutorHandler)
    {
        _registerTutorHandler = registerTutorHandler;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterTutor([FromBody] RegisterTutorRequest request, CancellationToken cancellationToken)
    {
        var clinicIdClaim = User.FindFirst("clinic_id")?.Value;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(clinicIdClaim, out var clinicId) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Token inválido ou sem ID da clínica." });
        }

        var command = new RegisterTutorCommand(
            clinicId, userId, request.Name, request.Cpf, request.Email, request.PhoneNumber, request.SecondaryPhoneNumber,
            request.ZipCode, request.Street, request.Number, request.Neighborhood, request.Complement, request.City,
            request.State, request.Notes
        );

        var result = await _registerTutorHandler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { result.ErrorCode, result.ErrorMessage });
        }

        return NoContent();
    }
}

public record RegisterTutorRequest(
    string Name, string Cpf, string? Email, string PhoneNumber, string? SecondaryPhoneNumber,
    string ZipCode, string Street, string Number, string Neighborhood, string? Complement, string City, string State, string? Notes
);