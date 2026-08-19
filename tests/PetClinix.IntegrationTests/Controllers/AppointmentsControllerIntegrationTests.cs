using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PetClinix.Modules.Appointments.Infrastructure.Persistence;
using PetClinix.Modules.Billing.Infrastructure.Persistence;
using PetClinix.Modules.Identity.Infrastructure.Persistence;
using Xunit;

namespace PetClinix.IntegrationTests.Controllers;

public class AppointmentsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AppointmentsControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(string Email, string Password, Guid UserId, Guid ClinicId)> SetupAdminAsync()
    {
        var email = $"admin_{Guid.NewGuid()}@teste.com";
        var password = "SenhaForte@123";

        var clinicRequest = new
        {
            TradeName = $"Clinica Appt {Guid.NewGuid().ToString().Substring(0, 8)}",
            LegalName = "Appt LTDA",
            DocumentNumber = Guid.NewGuid().ToString("N").Substring(0, 14),
            Email = $"clinica_{Guid.NewGuid()}@teste.com",
            PhoneNumber = "11988887777",
            ZipCode = "01001000",
            Street = "Rua Teste",
            Number = "123",
            Neighborhood = "Centro",
            City = "SP",
            State = "SP",
            AdminName = "Admin Appt",
            AdminEmail = email,
            AdminDocumentNumber = Guid.NewGuid().ToString("N").Substring(0, 11),
            AdminPhoneNumber = "11999990000",
            AdminBirthDate = new DateOnly(1990, 1, 1)
        };

        var clinicResponse = await _client.PostAsJsonAsync("/api/clinics", clinicRequest);
        var clinicResult = await clinicResponse.Content.ReadFromJsonAsync<RegisterClinicResponse>();
        var clinicId = clinicResult!.ClinicId;

        using (var scope = _factory.Services.CreateScope())
        {
            var billingDb = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
            var subscription = PetClinix.Modules.Billing.Domain.Entities.Subscription.Create(
                clinicId, $"cus_test_{Guid.NewGuid()}", $"sub_test_{Guid.NewGuid()}", PetClinix.Modules.Billing.Domain.Enums.PlanTier.Basic);
            await billingDb.Subscriptions.AddAsync(subscription);
            await billingDb.SaveChangesAsync();
        }

        var tokenResponse = await _client.GetAsync($"/api/users/{email}/generate-reset-token");
        var tokenResult = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        var setPasswordRequest = new { Token = tokenResult!.Token, Password = password };

        await _client.PostAsJsonAsync("/api/users/set-password", setPasswordRequest);

        Guid userId;
        using (var scope = _factory.Services.CreateScope())
        {
            var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            var emailVo = PetClinix.Modules.Identity.Domain.ValueObjects.Email.Create(email);
            var user = await identityDb.Users.FirstOrDefaultAsync(u => u.Email == emailVo);
            userId = user!.Id;
        }

        return (email, password, userId, clinicId);
    }

    [Fact]
    public async Task RegisterAppointment_Should_Return_401_When_No_Token_Provided()
    {
        var apptRequest = new
        {
            TutorId = Guid.NewGuid(),
            PetId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            VeterinarianId = Guid.NewGuid(),
            ScheduledDateUtc = DateTime.UtcNow.AddDays(1),
            Notes = "Sem token"
        };

        var response = await _client.PostAsJsonAsync("/api/appointments", apptRequest);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegisterAppointment_Should_Return_204_And_Save_In_Db_When_Valid()
    {
        var (email, password, userId, clinicId) = await SetupAdminAsync();
        var loginResponse = await _client.PostAsJsonAsync("/api/users/login", new { Email = email, Password = password });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);

        var vetId = Guid.NewGuid();
        var scheduledDate = DateTime.UtcNow.AddDays(2);

        var apptRequest = new
        {
            TutorId = Guid.NewGuid(),
            PetId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            VeterinarianId = vetId,
            ScheduledDateUtc = scheduledDate,
            Notes = "Consulta de rotina"
        };

        var response = await _client.PostAsJsonAsync("/api/appointments", apptRequest);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var apptDb = scope.ServiceProvider.GetRequiredService<AppointmentsDbContext>();
        var savedAppt = await apptDb.Appointments.FirstOrDefaultAsync(a => a.VeterinarianId == vetId);

        savedAppt.Should().NotBeNull();
        savedAppt!.ClinicId.Should().Be(clinicId);
        savedAppt.CreatedByUserId.Should().Be(userId);
        savedAppt.Status.Should().Be(PetClinix.Modules.Appointments.Domain.Enums.AppointmentStatus.Scheduled);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task RegisterAppointment_Should_Return_400_When_Double_Booking()
    {
        var (email, password, userId, clinicId) = await SetupAdminAsync();
        var loginResponse = await _client.PostAsJsonAsync("/api/users/login", new { Email = email, Password = password });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);

        var vetId = Guid.NewGuid();
        var scheduledDate = DateTime.UtcNow.AddDays(3);

        var apptRequest = new
        {
            TutorId = Guid.NewGuid(),
            PetId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            VeterinarianId = vetId,
            ScheduledDateUtc = scheduledDate,
            Notes = "Primeiro agendamento"
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/appointments", apptRequest);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var secondRequest = new
        {
            TutorId = Guid.NewGuid(), 
            PetId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            VeterinarianId = vetId, 
            ScheduledDateUtc = scheduledDate,
            Notes = "Tentativa de conflito"
        };

        var secondResponse = await _client.PostAsJsonAsync("/api/appointments", secondRequest);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorContent = await secondResponse.Content.ReadFromJsonAsync<ErrorResponse>();
        errorContent!.ErrorCode.Should().Be("appointments.appt.slot_taken");

        _client.DefaultRequestHeaders.Authorization = null;
    }
}