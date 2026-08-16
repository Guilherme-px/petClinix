using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PetClinix.Modules.Billing.Infrastructure.Persistence;
using PetClinix.Modules.Identity.Domain.ValueObjects;
using PetClinix.Modules.Identity.Infrastructure.Persistence;
using PetClinix.Modules.Pets.Infrastructure.Persistence;
using Xunit;

namespace PetClinix.IntegrationTests.Controllers;

public class PetsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public PetsControllerIntegrationTests(CustomWebApplicationFactory factory)
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
            TradeName = $"Clinica Pet {Guid.NewGuid().ToString().Substring(0, 8)}",
            LegalName = "Pet LTDA",
            DocumentNumber = Guid.NewGuid().ToString("N").Substring(0, 14),
            Email = $"clinica_{Guid.NewGuid()}@teste.com",
            PhoneNumber = "11988887777",
            ZipCode = "01001000",
            Street = "Rua Teste",
            Number = "123",
            Neighborhood = "Centro",
            City = "SP",
            State = "SP",
            AdminName = "Admin Pet",
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
            var emailVo = Email.Create(email);
            var user = await identityDb.Users.FirstOrDefaultAsync(u => u.Email == emailVo);
            userId = user!.Id;
        }

        return (email, password, userId, clinicId);
    }

    private async Task<Guid> SetupTutorAsync(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var tutorRequest = new
        {
            Name = "Tutor do Pet",
            Cpf = "12345678900",
            Email = (string?)null,
            PhoneNumber = "11988887777",
            SecondaryPhoneNumber = (string?)null,
            ZipCode = "01001000",
            Street = "Rua Teste",
            Number = "123",
            Neighborhood = "Centro",
            Complement = (string?)null,
            City = "Sao Paulo",
            State = "SP",
            Notes = (string?)null
        };

        await _client.PostAsJsonAsync("/api/tutors", tutorRequest);
        _client.DefaultRequestHeaders.Authorization = null;
        return Guid.Empty;
    }

    [Fact]
    public async Task RegisterPet_Should_Return_401_When_No_Token_Provided()
    {
        var petRequest = new
        {
            Name = "Rex",
            Species = 1,
            Breed = "Vira Lata",
            BirthDate = new DateOnly(2020, 5, 10),
            Sex = 1,
            Weight = 15.5,
            IsNeutered = true,
            Notes = "Nenhuma"
        };

        var response = await _client.PostAsJsonAsync($"/api/tutors/{Guid.NewGuid()}/pets", petRequest);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegisterPet_Should_Return_400_When_Tutor_Does_Not_Exist()
    {
        var (email, password, userId, clinicId) = await SetupAdminAsync();
        var loginResponse = await _client.PostAsJsonAsync("/api/users/login", new { Email = email, Password = password });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);

        var petRequest = new
        {
            Name = "Rex",
            Species = 1,
            Breed = "Vira Lata",
            BirthDate = new DateOnly(2020, 5, 10),
            Sex = 1,
            Weight = 15.5,
            IsNeutered = true,
            Notes = "Nenhuma"
        };

        var response = await _client.PostAsJsonAsync($"/api/tutors/{Guid.NewGuid()}/pets", petRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        errorContent!.ErrorCode.Should().Be("pets.tutor.not_found");

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task RegisterPet_Should_Return_204_And_Save_Pet_When_Valid()
    {
        var (email, password, userId, clinicId) = await SetupAdminAsync();
        var loginResponse = await _client.PostAsJsonAsync("/api/users/login", new { Email = email, Password = password });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);

        var tutorRequest = new
        {
            Name = "Tutor do Rex",
            Cpf = "12345678900",
            Email = (string?)null,
            PhoneNumber = "11988887777",
            SecondaryPhoneNumber = (string?)null,
            ZipCode = "01001000",
            Street = "Rua Teste",
            Number = "123",
            Neighborhood = "Centro",
            Complement = (string?)null,
            City = "Sao Paulo",
            State = "SP",
            Notes = (string?)null
        };

        await _client.PostAsJsonAsync("/api/tutors", tutorRequest);

        Guid tutorId;
        using (var scope = _factory.Services.CreateScope())
        {
            var petsDb = scope.ServiceProvider.GetRequiredService<PetsDbContext>();
            var savedTutor = await petsDb.Tutors.FirstOrDefaultAsync(t => t.ClinicId == clinicId);
            tutorId = savedTutor!.Id;
        }

        var petRequest = new
        {
            Name = "Rex",
            Species = 1,
            Breed = "Vira Lata",
            BirthDate = new DateOnly(2020, 5, 10),
            Sex = 1,
            Weight = 15.5,
            IsNeutered = true,
            Notes = "Agressivo com outros machos"
        };

        var response = await _client.PostAsJsonAsync($"/api/tutors/{tutorId}/pets", petRequest);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using (var scope = _factory.Services.CreateScope())
        {
            var petsDb = scope.ServiceProvider.GetRequiredService<PetsDbContext>();
            var savedPet = await petsDb.Pets.FirstOrDefaultAsync(p => p.TutorId == tutorId);

            savedPet.Should().NotBeNull();
            savedPet!.Name.Should().Be("Rex");
            savedPet.ClinicId.Should().Be(clinicId);
            savedPet.CreatedByUserId.Should().Be(userId);
            savedPet.IsNeutered.Should().BeTrue();
        }

        _client.DefaultRequestHeaders.Authorization = null;
    }
}