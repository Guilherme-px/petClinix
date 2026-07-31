using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace PetClinix.IntegrationTests.Controllers;

public class AuthControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AuthControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(string Email, string Password)> SetupUserWithPasswordAsync()
    {
        var email = $"login_{Guid.NewGuid()}@teste.com";
        var password = "SenhaForte@123";

        var clinicRequest = new
        {
            TradeName = $"Clinica Login {Guid.NewGuid().ToString().Substring(0, 8)}",
            LegalName = "Login LTDA",
            DocumentNumber = Guid.NewGuid().ToString("N").Substring(0, 14),
            Email = $"clinica_{Guid.NewGuid()}@teste.com",
            PhoneNumber = "11988887777",
            ZipCode = "01001000",
            Street = "Rua Teste",
            Number = "123",
            Neighborhood = "Centro",
            City = "SP",
            State = "SP",
            AdminName = "Admin Login",
            AdminEmail = email,
            AdminDocumentNumber = Guid.NewGuid().ToString("N").Substring(0, 11),
            AdminPhoneNumber = "11999990000",
            AdminBirthDate = new DateOnly(1990, 1, 1)
        };

        var clinicResponse = await _client.PostAsJsonAsync("/api/clinics", clinicRequest);

        if (!clinicResponse.IsSuccessStatusCode)
        {
            var error = await clinicResponse.Content.ReadAsStringAsync();
            throw new Exception($"Falha ao cadastrar clínica no setup do teste: {error}");
        }

        var tokenResponse = await _client.GetAsync($"/api/users/{email}/generate-reset-token");
        var tokenResult = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();

        var setPasswordRequest = new { Token = tokenResult!.Token, Password = password };
        await _client.PostAsJsonAsync("/api/users/set-password", setPasswordRequest);

        return (email, password);
    }

    [Fact]
    public async Task Login_Should_Return_200_And_Token_When_Credentials_Are_Valid()
    {
        var (email, password) = await SetupUserWithPasswordAsync();

        var loginRequest = new { Email = email, Password = password };
        var response = await _client.PostAsJsonAsync("/api/users/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.Email.Should().Be(email);
        result.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_Should_Return_401_When_Password_Is_Invalid()
    {
        var (email, _) = await SetupUserWithPasswordAsync();

        var loginRequest = new { Email = email, Password = "SenhaErrada@123" };
        var response = await _client.PostAsJsonAsync("/api/users/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_Should_Return_200_And_NewTokens_When_Valid()
    {
        var (email, password) = await SetupUserWithPasswordAsync();

        var loginRequest = new { Email = email, Password = password };
        var loginResponse = await _client.PostAsJsonAsync("/api/users/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var refreshRequest = new { RefreshToken = loginResult!.RefreshToken };
        var response = await _client.PostAsJsonAsync("/api/users/refresh", refreshRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();

        result.RefreshToken.Should().NotBe(loginResult.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_Should_Return_401_When_Invalid()
    {
        var refreshRequest = new { RefreshToken = "token-invalido-fake-123" };
        var response = await _client.PostAsJsonAsync("/api/users/refresh", refreshRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}