using Microsoft.AspNetCore.Mvc;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Billing.Application.UseCases.CreateCheckoutSession;

namespace PetClinix.Api.Controllers;

[ApiController]
[Route("api/subscriptions")]
public class BillingController : ControllerBase
{
    private readonly ICommandHandler<CreateCheckoutSessionCommand, Result<CreateCheckoutSessionResponse>> _handler;

    public BillingController(ICommandHandler<CreateCheckoutSessionCommand, Result<CreateCheckoutSessionResponse>> handler)
    {
        _handler = handler;
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCheckoutSessionCommand(
            request.ClinicId,
            request.PriceId,
            request.SuccessUrl,
            request.CancelUrl);

        var result = await _handler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { result.ErrorCode, result.ErrorMessage });
        }

        return Ok(result.Value);
    }
}

public record CreateCheckoutSessionRequest(Guid ClinicId, string PriceId, string SuccessUrl, string CancelUrl);