using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PetClinix.Modules.Billing.Domain.Entities;
using PetClinix.Modules.Billing.Domain.Enums;
using PetClinix.Modules.Billing.Infrastructure.Persistence;
using PetClinix.Modules.Identity.Domain.Enums;
using PetClinix.Modules.Identity.Infrastructure.Persistence;
using Xunit;

namespace PetClinix.IntegrationTests.Controllers;

public class StaffControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public StaffControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(string Email, string Password)> SetupAdminWithSubscriptionAsync()
    {
        var email = $"admin_{Guid.NewGuid()}@teste.com";
        var password = "SenhaNova@123";

        var clinicRequest = new
        {
            TradeName = $"Clinica Staff {Guid.NewGuid().ToString().Substring(0, 8)}",
            LegalName = "Staff LTDA",
            DocumentNumber = Guid.NewGuid().ToString("N").Substring(0, 14),
            Email = $"clinica_{Guid.NewGuid()}@teste.com",
            PhoneNumber = "11988887777",
            ZipCode = "01001000",
            Street = "Rua Teste",
            Number = "123",
            Neighborhood = "Centro",
            City = "SP",
            State = "SP",
            AdminName = "Admin Staff",
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
            var subscription = Subscription.Create(clinicId, $"cus_test_{Guid.NewGuid()}", $"sub_test_{Guid.NewGuid()}", PlanTier.Basic);
            await billingDb.Subscriptions.AddAsync(subscription);
            await billingDb.SaveChangesAsync();
        }

        var tokenResponse = await _client.GetAsync($"/api/users/{email}/generate-reset-token");
        var tokenResult = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();

        var setPasswordRequest = new { Token = tokenResult!.Token, Password = password };
        await _client.PostAsJsonAsync("/api/users/set-password", setPasswordRequest);

        return (email, password);
    }

    [Fact]
    public async Task RegisterStaff_Should_Return_401_When_No_Token_Provided()
    {
        var staffRequest = new
        {
            Name = "Dr. Teste",
            Email = "dr@teste.com",
            DocumentNumber = "12345678900",
            PhoneNumber = "11999990000",
            BirthDate = new DateOnly(1990, 1, 1),
            Role = "Veterinarian"
        };

        var response = await _client.PostAsJsonAsync("/api/clinics/me/staff", staffRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegisterStaff_Should_Return_204_And_Create_User_When_Valid()
    {
        var (email, password) = await SetupAdminWithSubscriptionAsync();

        var loginRequest = new { Email = email, Password = password };
        var loginResponse = await _client.PostAsJsonAsync("/api/users/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);

        var staffEmail = $"vet_{Guid.NewGuid()}@teste.com";
        var staffRequest = new
        {
            Name = "Dr. Dolittle",
            Email = staffEmail,
            DocumentNumber = "98765432100",
            PhoneNumber = "11988887777",
            BirthDate = new DateOnly(1985, 5, 10),
            Role = "Veterinarian"
        };

        var response = await _client.PostAsJsonAsync("/api/clinics/me/staff", staffRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var emailVo = PetClinix.Modules.Identity.Domain.ValueObjects.Email.Create(staffEmail);
        var savedUser = await identityDb.Users.FirstOrDefaultAsync(u => u.Email == emailVo);

        savedUser.Should().NotBeNull();
        savedUser!.Role.Should().Be(UserRole.Veterinarian);
        savedUser.PasswordHash.Should().BeNull();

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task RegisterStaff_Should_Return_400_When_Plan_Limit_Reached()
    {
        var (email, password) = await SetupAdminWithSubscriptionAsync();

        var loginRequest = new { Email = email, Password = password };
        var loginResponse = await _client.PostAsJsonAsync("/api/users/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);

        for (int i = 0; i < 3; i++)
        {
            var staffRequest = new
            {
                Name = $"Staff {i}",
                Email = $"staff_{i}_{Guid.NewGuid()}@teste.com",
                DocumentNumber = Guid.NewGuid().ToString("N").Substring(0, 11),
                PhoneNumber = "11999990000",
                BirthDate = new DateOnly(1990, 1, 1),
                Role = "Receptionist"
            };
            await _client.PostAsJsonAsync("/api/clinics/me/staff", staffRequest);
        }

        var limitRequest = new
        {
            Name = "Staff 5",
            Email = $"staff5_{Guid.NewGuid()}@teste.com",
            DocumentNumber = "11122233344",
            PhoneNumber = "11999990000",
            BirthDate = new DateOnly(1990, 1, 1),
            Role = "Receptionist"
        };

        var limitResponse = await _client.PostAsJsonAsync("/api/clinics/me/staff", limitRequest);

        limitResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await limitResponse.Content.ReadFromJsonAsync<ErrorResponse>();
        errorContent!.ErrorCode.Should().Be("identity.staff_limit_reached");

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task GetStaff_Should_Return_401_When_No_Token_Provided()
    {
        var response = await _client.GetAsync("/api/clinics/me/staff");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetStaff_Should_Return_200_And_Staff_List_When_Valid()
    {
        var (email, password) = await SetupAdminWithSubscriptionAsync();

        var loginRequest = new { Email = email, Password = password };
        var loginResponse = await _client.PostAsJsonAsync("/api/users/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);

        var response = await _client.GetAsync("/api/clinics/me/staff");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedStaffResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(1);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task GetStaffById_Should_Return_404_When_User_Does_Not_Exist()
    {
        var (email, password) = await SetupAdminWithSubscriptionAsync();
        var loginRequest = new { Email = email, Password = password };
        var loginResponse = await _client.PostAsJsonAsync("/api/users/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);

        var fakeUserId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/clinics/me/staff/{fakeUserId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _client.DefaultRequestHeaders.Authorization = null;
    }
}

public class StaffItemResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class PagedStaffResponse
{
    public List<StaffItemResponse> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}