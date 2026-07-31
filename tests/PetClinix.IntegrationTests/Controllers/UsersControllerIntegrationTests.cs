using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PetClinix.Modules.Identity.Domain.ValueObjects;
using PetClinix.Modules.Identity.Infrastructure.Persistence;
using Xunit;

namespace PetClinix.IntegrationTests.Controllers;

public class UsersControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public UsersControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SetPassword_Should_Update_User_Hash_In_Database()
    {
        var clinicRequest = new
        {
            TradeName = "Clinica Senha Teste",
            LegalName = "Senha LTDA",
            DocumentNumber = "12345678000199",
            Email = "clinicatestes@senha.com",
            PhoneNumber = "11988887777",
            ZipCode = "01001000",
            Street = "Rua Teste",
            Number = "123",
            Neighborhood = "Centro",
            City = "SP",
            State = "SP",
            AdminName = "Admin Senha",
            AdminEmail = "admin@senha.com",
            AdminDocumentNumber = "12345678900",
            AdminPhoneNumber = "11999990000",
            AdminBirthDate = new DateOnly(1990, 1, 1)
        };

        var clinicResponse = await _client.PostAsJsonAsync("/api/clinics", clinicRequest);
        clinicResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var tokenResponse = await _client.GetAsync("/api/users/admin@senha.com/generate-reset-token");
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var tokenResult = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        tokenResult.Should().NotBeNull();
        var token = tokenResult!.Token;

        var setPasswordRequest = new
        {
            Token = token,
            Password = "NovaSenhaForte@123"
        };

        var setPassResponse = await _client.PostAsJsonAsync("/api/users/set-password", setPasswordRequest);
        setPassResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        var emailVo = Email.Create("admin@senha.com");
        var savedUser = await db.Users.FirstOrDefaultAsync(u => u.Email == emailVo);

        savedUser.Should().NotBeNull();
        savedUser!.PasswordHash.Should().NotBeNullOrEmpty();
        savedUser.PasswordResetToken.Should().BeNull();
        savedUser.PasswordResetTokenExpiresAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task SetPassword_Should_Return_400_When_Token_Is_Invalid()
    {
        var setPasswordRequest = new
        {
            Token = "token-invalido-inexistente",
            Password = "NovaSenhaForte@123"
        };

        var response = await _client.PostAsJsonAsync("/api/users/set-password", setPasswordRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

public class TokenResponse
{
    public string Token { get; set; } = string.Empty;
}