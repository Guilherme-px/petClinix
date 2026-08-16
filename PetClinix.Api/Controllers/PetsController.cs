using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Pets.Application.UseCases.RegisterPet;
using PetClinix.Modules.Pets.Domain.Enums;
using System.Security.Claims;

namespace PetClinix.Api.Controllers;

[ApiController]
[Route("api/tutors/{tutorId}/pets")]
[Authorize]
public class PetsController : ControllerBase
{
    private readonly ICommandHandler<RegisterPetCommand, Result> _registerPetHandler;

    public PetsController(ICommandHandler<RegisterPetCommand, Result> registerPetHandler)
    {
        _registerPetHandler = registerPetHandler;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterPet(Guid tutorId, [FromBody] RegisterPetRequest request, CancellationToken cancellationToken)
    {
        var clinicIdClaim = User.FindFirst("clinic_id")?.Value;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(clinicIdClaim, out var clinicId) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Token inválido." });
        }

        var command = new RegisterPetCommand(
            clinicId,
            tutorId,
            userId,
            request.Name,
            request.Species,
            request.Breed,
            request.BirthDate,
            request.Sex,
            request.Weight,
            request.IsNeutered,
            request.Notes
        );

        var result = await _registerPetHandler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { result.ErrorCode, result.ErrorMessage });
        }

        return NoContent();
    }
}

public record RegisterPetRequest(
    string Name,
    Species Species,
    string? Breed,
    DateOnly? BirthDate,
    PetSex Sex,
    double? Weight,
    bool IsNeutered,
    string? Notes
);