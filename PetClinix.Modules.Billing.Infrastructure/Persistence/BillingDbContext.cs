using Microsoft.EntityFrameworkCore;
using PetClinix.Modules.Billing.Domain.Entities;

namespace PetClinix.Modules.Billing.Infrastructure.Persistence;

public class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options) { }

    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<WebhookEvent> WebhookEvents => Set<WebhookEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.ToTable("Subscriptions");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.StripeCustomerId).HasMaxLength(100).IsRequired();
            entity.Property(s => s.StripeSubscriptionId).HasMaxLength(100).IsRequired();
            entity.Property(s => s.Status).IsRequired();
            entity.HasIndex(s => s.ClinicId).IsUnique();
            entity.HasIndex(s => s.StripeSubscriptionId).IsUnique();
        });

        modelBuilder.Entity<WebhookEvent>(entity =>
       {
           entity.ToTable("WebhookEvents");
           entity.HasKey(w => w.Id);
           entity.Property(w => w.StripeEventId).HasMaxLength(100).IsRequired();
           entity.HasIndex(w => w.StripeEventId).IsUnique();
       });
    }
}