using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Catalog.Application.UseCases.RegisterService;
using PetClinix.Modules.Catalog.Application.UseCases.GetServices;
using PetClinix.Modules.Catalog.Application.UseCases.UpdateService;
using PetClinix.Modules.Catalog.Application.UseCases.DeactivateService;
using System.Security.Claims;

namespace PetClinix.Api.Controllers;

[ApiController]
[Route("api/services")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly ICommandHandler<RegisterServiceCommand, Result> _registerServiceHandler;
    private readonly ICommandHandler<GetServicesQuery, Result<PagedResult<ServiceResponse>>> _getServicesHandler;
    private readonly ICommandHandler<UpdateServiceCommand, Result> _updateServiceHandler;
    private readonly ICommandHandler<DeactivateServiceCommand, Result> _deactivateServiceHandler;

    public ServicesController(
        ICommandHandler<RegisterServiceCommand, Result> registerServiceHandler,
        ICommandHandler<GetServicesQuery, Result<PagedResult<ServiceResponse>>> getServicesHandler,
        ICommandHandler<UpdateServiceCommand, Result> updateServiceHandler,
        ICommandHandler<DeactivateServiceCommand, Result> deactivateServiceHandler)
    {
        _registerServiceHandler = registerServiceHandler;
        _getServicesHandler = getServicesHandler;
        _updateServiceHandler = updateServiceHandler;
        _deactivateServiceHandler = deactivateServiceHandler;
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

    [HttpGet]
    public async Task<IActionResult> GetServices([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var clinicIdClaim = User.FindFirst("clinic_id")?.Value;
        if (!Guid.TryParse(clinicIdClaim, out var clinicId))
        {
            return Unauthorized(new { message = "Token inválido ou sem ID da clínica." });
        }

        var query = new GetServicesQuery(clinicId, pageNumber, pageSize);
        var result = await _getServicesHandler.Handle(query, cancellationToken);

        return Ok(result.Value);
    }

    [HttpPut("{serviceId}")]
    public async Task<IActionResult> UpdateService(Guid serviceId, [FromBody] UpdateServiceRequest request, CancellationToken cancellationToken)
    {
        var clinicIdClaim = User.FindFirst("clinic_id")?.Value;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(clinicIdClaim, out var clinicId) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Token inválido." });
        }

        var command = new UpdateServiceCommand(
            clinicId, serviceId, userId,
            request.Name, request.Description, request.DurationInMinutes, request.Price, request.RequiresVeterinarian);

        var result = await _updateServiceHandler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { result.ErrorCode, result.ErrorMessage });
        }

        return NoContent();
    }

    [HttpDelete("{serviceId}")]
    public async Task<IActionResult> DeactivateService(Guid serviceId, CancellationToken cancellationToken)
    {
        var clinicIdClaim = User.FindFirst("clinic_id")?.Value;
        
        if (!Guid.TryParse(clinicIdClaim, out var clinicId))
        {
            return Unauthorized(new { message = "Token inválido ou sem ID da clínica." });
        }

        var command = new DeactivateServiceCommand(clinicId, serviceId);
        var result = await _deactivateServiceHandler.Handle(command, cancellationToken);

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

public record UpdateServiceRequest(
    string Name,
    string? Description,
    int DurationInMinutes,
    decimal Price,
    bool RequiresVeterinarian
);