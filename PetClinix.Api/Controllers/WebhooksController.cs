using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Billing.Application.UseCases.ActivateSubscription;
using PetClinix.Modules.Billing.Application.UseCases.CancelSubscription;
using Stripe;
using Stripe.Checkout;

namespace PetClinix.Api.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ICommandHandler<ActivateSubscriptionCommand, Result> _activateHandler;
    private readonly ILogger<WebhooksController> _logger;
    private readonly ICommandHandler<CancelSubscriptionCommand, Result> _cancelHandler;

    public WebhooksController(
        IConfiguration configuration,
        ICommandHandler<ActivateSubscriptionCommand, Result> activateHandler,
        ICommandHandler<CancelSubscriptionCommand, Result> cancelHandler,
        ILogger<WebhooksController> logger)
    {
        _configuration = configuration;
        _activateHandler = activateHandler;
        _cancelHandler = cancelHandler;
        _logger = logger;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"];
        var webhookSecret = _configuration["Stripe:WebhookSecret"];

        Event stripeEvent;

        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);
        }
        catch (StripeException e)
        {
            _logger.LogError(e, "Erro ao validar assinatura do webhook do Stripe.");
            return BadRequest(new { error = "Invalid signature" });
        }

        if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
        {
            var session = stripeEvent.Data.Object as Session;

            var clinicId = session?.ClientReferenceId;

            var stripeCustomerId = session?.CustomerId;
            var stripeSubscriptionId = session?.SubscriptionId;

            if (Guid.TryParse(clinicId, out var parsedClinicId) && !string.IsNullOrEmpty(stripeCustomerId) && !string.IsNullOrEmpty(stripeSubscriptionId))
            {
                _logger.LogInformation("Pagamento aprovado para a clínica ID: {ClinicId}", parsedClinicId);

                var command = new ActivateSubscriptionCommand(parsedClinicId, stripeCustomerId, stripeSubscriptionId);
                var result = await _activateHandler.Handle(command, HttpContext.RequestAborted);

                if (result.IsFailure)
                {
                    _logger.LogError("Erro ao ativar assinatura: {ErrorMessage}", result.ErrorMessage);
                    return StatusCode(500, new { error = "Failed to activate subscription" });
                }
            }
            else
            {
                _logger.LogWarning("Webhook recebido, mas faltam dados do checkout.");
            }
        }
        else if (stripeEvent.Type == EventTypes.CustomerSubscriptionDeleted) 
        {
            var subscription = stripeEvent.Data.Object as Stripe.Subscription;
            var stripeSubscriptionId = subscription?.Id;

            if (!string.IsNullOrEmpty(stripeSubscriptionId))
            {
                _logger.LogInformation("❌ Assinatura cancelada no Stripe: {SubscriptionId}", stripeSubscriptionId);

                var command = new CancelSubscriptionCommand(stripeSubscriptionId);
                var result = await _cancelHandler.Handle(command, HttpContext.RequestAborted);

                if (result.IsFailure)
                {
                    _logger.LogError("Erro ao cancelar assinatura no banco: {ErrorMessage}", result.ErrorMessage);
                    return StatusCode(500, new { error = "Failed to cancel subscription" });
                }
            }
        }
        else
        {
            _logger.LogInformation("Evento recebido do Stripe (ignorado): {EventType}", stripeEvent.Type);
        }

        return Ok();
    }
}