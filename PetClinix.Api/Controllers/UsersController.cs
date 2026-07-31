using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Application.UseCases.SetPassword;
using PetClinix.Modules.Identity.Application.UseCases.Login;
using PetClinix.Modules.Identity.Domain.Repositories;
using PetClinix.Modules.Identity.Domain.ValueObjects;

namespace PetClinix.Api.Controllers;

[ApiController]
[Route("api/users")]
[EnableRateLimiting("LoginPolicy")]
public class UsersController : ControllerBase
{
    private readonly ICommandHandler<SetPasswordCommand, Result> _setPasswordHandler;
    private readonly IUserRepository _userRepository;
    private readonly ICommandHandler<LoginCommand, Result<LoginResponse>> _loginHandler;

    public UsersController(
        ICommandHandler<SetPasswordCommand, Result> setPasswordHandler,
        IUserRepository userRepository,
        ICommandHandler<LoginCommand, Result<LoginResponse>> loginHandler)
    {
        _setPasswordHandler = setPasswordHandler;
        _userRepository = userRepository;
        _loginHandler = loginHandler;
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
}

public record SetPasswordRequest(string Token, string Password);
public record LoginRequest(string Email, string Password);