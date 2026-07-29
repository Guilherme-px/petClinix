using PetClinix.BuildingBlocks.Domain;
using PetClinix.Modules.Billing.Domain.Enums;

namespace PetClinix.Modules.Billing.Domain.Entities;

public sealed class Subscription : AggregateRoot
{
    public Guid ClinicId { get; private set; }
    public string StripeCustomerId { get; private set; } = string.Empty;
    public string StripeSubscriptionId { get; private set; } = string.Empty;
    public SubscriptionStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private Subscription() { }

    private Subscription(Guid clinicId, string stripeCustomerId, string stripeSubscriptionId, SubscriptionStatus status)
    {
        Id = Guid.NewGuid();
        ClinicId = clinicId;
        StripeCustomerId = stripeCustomerId;
        StripeSubscriptionId = stripeSubscriptionId;
        Status = status;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Subscription Create(Guid clinicId, string stripeCustomerId, string stripeSubscriptionId)
    {
        return new Subscription(clinicId, stripeCustomerId, stripeSubscriptionId, SubscriptionStatus.Active);
    }

    public void MarkAsCanceled()
    {
        Status = SubscriptionStatus.Canceled;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}