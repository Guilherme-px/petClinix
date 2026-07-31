using Microsoft.AspNetCore.Mvc;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Application.UseCases.SetPassword;
using PetClinix.Modules.Identity.Domain.Repositories;
using PetClinix.Modules.Identity.Domain.ValueObjects;

namespace PetClinix.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly ICommandHandler<SetPasswordCommand, Result> _setPasswordHandler;
    private readonly IUserRepository _userRepository;

    public UsersController(
        ICommandHandler<SetPasswordCommand, Result> setPasswordHandler,
        IUserRepository userRepository)
    {
        _setPasswordHandler = setPasswordHandler;
        _userRepository = userRepository;
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