using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Application.UseCases.UpdateClinic;
using System.Security.Claims;

namespace PetClinix.Api.Controllers;

[ApiController]
[Route("api/clinics")]
public class ClinicsController : ControllerBase
{
    private readonly ICommandHandler<UpdateClinicCommand, Result> _updateClinicHandler;

    public ClinicsController(ICommandHandler<UpdateClinicCommand, Result> updateClinicHandler)
    {
        _updateClinicHandler = updateClinicHandler;
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateClinic([FromBody] UpdateClinicRequest request, CancellationToken cancellationToken)
    {
        var clinicIdClaim = User.FindFirst("clinic_id")?.Value;

        if (!Guid.TryParse(clinicIdClaim, out var clinicId))
        {
            return Unauthorized(new { message = "Token inválido ou sem ID da clínica." });
        }

        var command = new UpdateClinicCommand(
            clinicId,
            request.TradeName, request.LegalName, request.DocumentNumber,
            request.Email, request.PhoneNumber,
            request.ZipCode, request.Street, request.Number, request.Neighborhood,
            request.Complement, request.City, request.State);

        var result = await _updateClinicHandler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { result.ErrorCode, result.ErrorMessage });
        }

        return NoContent();
    }
}

public record UpdateClinicRequest(
    string TradeName, string LegalName, string DocumentNumber,
    string Email, string PhoneNumber,
    string ZipCode, string Street, string Number, string Neighborhood,
    string? Complement, string City, string State);