using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Billing.Infrastructure.Persistence;
using PetClinix.Modules.Catalog.Infrastructure.Persistence;
using PetClinix.Modules.Identity.Infrastructure.Persistence;
using PetClinix.Modules.Pets.Infrastructure.Persistence;
using PetClinix.Modules.Appointments.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace PetClinix.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
#pragma warning disable CS0618
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("petclinix_test_db")
        .WithUsername("testuser")
        .WithPassword("testpassword")
        .Build();
#pragma warning restore CS0618

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            var identityDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<IdentityDbContext>));
            if (identityDescriptor != null) services.Remove(identityDescriptor);

            services.AddDbContext<IdentityDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
                options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });

            var billingDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BillingDbContext>));
            if (billingDescriptor != null) services.Remove(billingDescriptor);

            services.AddDbContext<BillingDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
                options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });

            var petsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PetsDbContext>));
            if (petsDescriptor != null) services.Remove(petsDescriptor);

            services.AddDbContext<PetsDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
                options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });

            var catalogDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<CatalogDbContext>));
            if (catalogDescriptor != null) services.Remove(catalogDescriptor);

            services.AddDbContext<CatalogDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
                options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });

            var apptDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppointmentsDbContext>));
            if (apptDescriptor != null) services.Remove(apptDescriptor);

            services.AddDbContext<AppointmentsDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
                options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });

            var emailServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailService));
            if (emailServiceDescriptor != null) services.Remove(emailServiceDescriptor);
            services.AddScoped<IEmailService, TestEmailService>();

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();

            var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            var billingDb = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
            var petsDb = scope.ServiceProvider.GetRequiredService<PetsDbContext>();
            var catalogDb = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var apptDb = scope.ServiceProvider.GetRequiredService<AppointmentsDbContext>();

            identityDb.Database.Migrate();
            billingDb.Database.Migrate();
            petsDb.Database.Migrate();
            catalogDb.Database.Migrate();
            apptDb.Database.Migrate();
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}