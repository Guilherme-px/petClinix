using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Catalog.Application.UseCases.RegisterService;
using System.Security.Claims;

namespace PetClinix.Api.Controllers;

[ApiController]
[Route("api/services")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly ICommandHandler<RegisterServiceCommand, Result> _registerServiceHandler;

    public ServicesController(ICommandHandler<RegisterServiceCommand, Result> registerServiceHandler)
    {
        _registerServiceHandler = registerServiceHandler;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterService([FromBody] RegisterServiceRequest request, CancellationToken cancellationToken)
    {
        var clinicIdClaim = User.FindFirst("clinic_id")?.Value;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(clinicIdClaim, out var clinicId) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Token inválido." });
        }

        var command = new RegisterServiceCommand(
            clinicId,
            userId,
            request.Name,
            request.Description,
            request.DurationInMinutes,
            request.Price,
            request.RequiresVeterinarian
        );

        var result = await _registerServiceHandler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { result.ErrorCode, result.ErrorMessage });
        }

        return NoContent();
    }
}

public record RegisterServiceRequest(
    string Name,
    string? Description,
    int DurationInMinutes,
    decimal Price,
    bool RequiresVeterinarian
);