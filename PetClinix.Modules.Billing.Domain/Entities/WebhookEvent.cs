using PetClinix.BuildingBlocks.Domain;

namespace PetClinix.Modules.Billing.Domain.Entities;

public sealed class WebhookEvent : Entity
{
    public string StripeEventId { get; private set; }
    public DateTime ProcessedAtUtc { get; private set; }

#pragma warning disable CS8618
    private WebhookEvent() { }

    private WebhookEvent(string stripeEventId)
    {
        Id = Guid.NewGuid();
        StripeEventId = stripeEventId;
        ProcessedAtUtc = DateTime.UtcNow;
    }

    public static WebhookEvent Create(string stripeEventId)
    {
        if (string.IsNullOrWhiteSpace(stripeEventId))
            throw new ArgumentException("Stripe Event ID is required.", nameof(stripeEventId));

        return new WebhookEvent(stripeEventId);
    }
}