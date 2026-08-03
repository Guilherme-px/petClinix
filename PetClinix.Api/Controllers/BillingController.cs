using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Billing.Application.UseCases.CreateCheckoutSession;
using PetClinix.Modules.Billing.Application.UseCases.CreatePortalSession;
using System.Security.Claims;

namespace PetClinix.Api.Controllers;

[ApiController]
[Route("api/subscriptions")]
public class BillingController : ControllerBase
{
    private readonly ICommandHandler<CreateCheckoutSessionCommand, Result<CreateCheckoutSessionResponse>> _handler;
    private readonly ICommandHandler<CreatePortalSessionCommand, Result<CreatePortalSessionResponse>> _portalHandler;

    public BillingController(
        ICommandHandler<CreateCheckoutSessionCommand, Result<CreateCheckoutSessionResponse>> handler,
        ICommandHandler<CreatePortalSessionCommand, Result<CreatePortalSessionResponse>> portalHandler)
    {
        _handler = handler;
        _portalHandler = portalHandler;
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCheckoutSessionCommand(
            request.ClinicId,
            request.PriceId,
            request.AdminEmail,
            request.SuccessUrl,
            request.CancelUrl);

        var result = await _handler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { result.ErrorCode, result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("portal")]
    public async Task<IActionResult> CreateBillingPortal([FromBody] CreatePortalSessionRequest request, CancellationToken cancellationToken)
    {
        var clinicIdClaim = User.FindFirst("clinic_id")?.Value;

        if (!Guid.TryParse(clinicIdClaim, out var clinicId))
        {
            return Unauthorized(new { message = "Token inválido ou sem ID da clínica." });
        }

        var command = new CreatePortalSessionCommand(clinicId, request.ReturnUrl);
        var result = await _portalHandler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { result.ErrorCode, result.ErrorMessage });
        }

        return Ok(result.Value);
    }
}

public record CreateCheckoutSessionRequest(Guid ClinicId, string PriceId, string AdminEmail, string SuccessUrl, string CancelUrl);
public record CreatePortalSessionRequest(string ReturnUrl);