using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PetClinix.Modules.Identity.Application.UseCases.GetProfile;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Application.UseCases.SetPassword;
using PetClinix.Modules.Identity.Application.UseCases.Login;
using PetClinix.Modules.Identity.Domain.Repositories;
using PetClinix.Modules.Identity.Domain.ValueObjects;
using PetClinix.Modules.Identity.Application.UseCases.RefreshToken;

namespace PetClinix.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly ICommandHandler<SetPasswordCommand, Result> _setPasswordHandler;
    private readonly IUserRepository _userRepository;
    private readonly ICommandHandler<LoginCommand, Result<LoginResponse>> _loginHandler;
    private readonly ICommandHandler<RefreshTokenCommand, Result<RefreshTokenResponse>> _refreshTokenHandler;
    private readonly ICommandHandler<GetProfileQuery, Result<ProfileResponse>> _getProfileHandler;

    public UsersController(
        ICommandHandler<SetPasswordCommand, Result> setPasswordHandler,
        IUserRepository userRepository,
        ICommandHandler<LoginCommand, Result<LoginResponse>> loginHandler,
        ICommandHandler<GetProfileQuery, Result<ProfileResponse>> getProfileHandler,
        ICommandHandler<RefreshTokenCommand, Result<RefreshTokenResponse>> refreshTokenHandler)
    {
        _setPasswordHandler = setPasswordHandler;
        _userRepository = userRepository;
        _loginHandler = loginHandler;
        _getProfileHandler = getProfileHandler;
        _refreshTokenHandler = refreshTokenHandler;
    }

    [HttpPost("set-password")]
    public async Task<IActionResult> SetPassword([FromBody] SetPasswordRequest request, CancellationToken cancellationToken)
    {
        var command = new SetPasswordCommand(request.Token, request.Password);
        var result = await _setPasswordHandler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { result.ErrorCode, result.ErrorMessage });
        }

        return Ok(new { message = "Senha definida com sucesso!" });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await _refreshTokenHandler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(new { result.ErrorCode, result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    [EnableRateLimiting("LoginPolicy")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _loginHandler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(new { result.ErrorCode, result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    [HttpGet("{email}/generate-reset-token")]
    public async Task<IActionResult> GenerateResetToken(string email)
    {
        var emailVo = Email.Create(email);

        var user = await _userRepository.GetByEmailAsync(emailVo, CancellationToken.None);
        if (user == null) return NotFound("Usuário não encontrado");

        var token = user.GeneratePasswordResetToken();
        await _userRepository.UpdateAsync(user, CancellationToken.None);

        return Ok(new { token });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Token inválido ou sem ID do usuário." });
        }

        var query = new GetProfileQuery(userId);
        var result = await _getProfileHandler.Handle(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { result.ErrorCode, result.ErrorMessage });
        }

        return Ok(result.Value);
    }
}

public record SetPasswordRequest(string Token, string Password);
public record LoginRequest(string Email, string Password);
public record RefreshTokenRequest(string RefreshToken);