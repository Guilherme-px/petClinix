using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PetClinix.Modules.Identity.Infrastructure.Persistence;
using Xunit;

namespace PetClinix.IntegrationTests.Controllers;

public class ClinicsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ClinicsControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private record TestClinicRequest(
        string TradeName, string LegalName, string DocumentNumber,
        string Email, string PhoneNumber,
        string ZipCode, string Street, string Number, string Neighborhood,
        string? Complement, string City, string State,
        string AdminName, string AdminEmail,
        string AdminDocumentNumber, string AdminPhoneNumber, DateOnly AdminBirthDate);

    private static string RandomNumbers(int length) => Guid.NewGuid().ToString("N").Substring(0, length);

    private static TestClinicRequest CreateValidRequest() => new(
        $"Clinica Teste {Guid.NewGuid()}", "Teste E2E LTDA", RandomNumbers(14),
        $"{Guid.NewGuid()}@teste.com", "11988887777",
        "01001000", "Rua Teste", "123", "Centro",
        "Sala 1", "Sao Paulo", "SP",
        "Admin E2E", $"{Guid.NewGuid()}@admin.com",
        RandomNumbers(11), "11999990000", new DateOnly(1990, 1, 1));

    [Fact]
    public async Task Post_Clinic_Should_Return_201_And_Save_In_Database()
    {
        var request = CreateValidRequest();

        var response = await _client.PostAsJsonAsync("/api/clinics", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<RegisterClinicResponse>();
        result.Should().NotBeNull();
        result!.ClinicId.Should().NotBeEmpty();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        var savedClinic = await db.Clinics.FirstOrDefaultAsync(c => c.Id == result.ClinicId);
        var savedUser = await db.Users.FirstOrDefaultAsync(u => u.Id == result.AdminUserId);

        savedClinic.Should().NotBeNull();
        savedClinic!.TradeName.Should().Be(request.TradeName);
        savedClinic.Slug.Value.Should().StartWith("clinica-teste-");

        savedUser.Should().NotBeNull();
        savedUser!.PasswordHash.Should().BeNull();
    }

    [Fact]
    public async Task Post_Clinic_Should_Return_400_When_Email_Already_Exists()
    {
        var firstRequest = CreateValidRequest();

        var firstResponse = await _client.PostAsJsonAsync("/api/clinics", firstRequest);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var secondRequest = firstRequest with
        {
            TradeName = $"Outro Nome {Guid.NewGuid()}",
            LegalName = $"Outra Razao {Guid.NewGuid()}"
        };

        var secondResponse = await _client.PostAsJsonAsync("/api/clinics", secondRequest);

        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await secondResponse.Content.ReadFromJsonAsync<ErrorResponse>();
        errorContent.Should().NotBeNull();
        errorContent!.ErrorCode.Should().Be("identity.clinic.email_already_exists");
    }

    [Fact]
    public async Task Post_Clinic_Should_Return_400_When_TradeName_Is_Missing()
    {
        var request = CreateValidRequest() with { TradeName = "" };

        var response = await _client.PostAsJsonAsync("/api/clinics", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        errorContent.Should().NotBeNull();
        errorContent!.ErrorCode.Should().Be("identity.clinic_slug.required");
    }
}

public class RegisterClinicResponse
{
    public Guid ClinicId { get; set; }
    public Guid AdminUserId { get; set; }
}

public class ErrorResponse
{
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}