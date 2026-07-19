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

    [Fact]
    public async Task Post_Clinic_Should_Return_201_And_Save_In_Database()
    {
        var request = new
        {
            TradeName = "Clinica Teste E2E",
            LegalName = "Teste E2E LTDA",
            DocumentNumber = "12345678000199",
            Email = "e2e@teste.com",
            PhoneNumber = "11988887777",
            ZipCode = "01001000",
            Street = "Rua Teste",
            Number = "123",
            Neighborhood = "Centro",
            Complement = "Sala 1",
            City = "Sao Paulo",
            State = "SP",
            AdminName = "Admin E2E",
            AdminEmail = "admin@e2e.com",
            AdminDocumentNumber = "12345678900",
            AdminPhoneNumber = "11999990000",
            AdminBirthDate = new DateOnly(1990, 1, 1)
        };

        var response = await _client.PostAsJsonAsync("/api/clinics", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API returned {response.StatusCode}: {errorContent}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<RegisterClinicResponse>();
        result.Should().NotBeNull();
        result!.ClinicId.Should().NotBeEmpty();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        
        var savedClinic = await db.Clinics.FirstOrDefaultAsync(c => c.Id == result.ClinicId);
        var savedUser = await db.Users.FirstOrDefaultAsync(u => u.Id == result.AdminUserId);

        savedClinic.Should().NotBeNull();
        savedClinic!.TradeName.Should().Be("Clinica Teste E2E");
        savedClinic.Slug.Value.Should().Be("clinica-teste-e2e");

        savedUser.Should().NotBeNull();
        savedUser!.PasswordHash.Should().BeNull();
    }
}

public class RegisterClinicResponse
{
    public Guid ClinicId { get; set; }
    public Guid AdminUserId { get; set; }
}