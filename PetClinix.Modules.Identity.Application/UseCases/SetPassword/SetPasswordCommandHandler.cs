using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Application.Contracts;
using PetClinix.Modules.Identity.Domain.Exceptions;
using PetClinix.Modules.Identity.Domain.Repositories;

namespace PetClinix.Modules.Identity.Application.UseCases.SetPassword;

public sealed class SetPasswordCommandHandler : ICommandHandler<SetPasswordCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public SetPasswordCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> Handle(SetPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByPasswordResetTokenAsync(command.Token, cancellationToken);

        if (user == null)
        {
            return Result.Failure("identity.user.invalid_token", "Token de redefinição inválido.");
        }

        try
        {
            var passwordHash = _passwordHasher.Hash(command.Password);
            user.SetPassword(command.Token, passwordHash);

            await _userRepository.UpdateAsync(user, cancellationToken);
            return Result.Success();
        }
        catch (IdentityDomainException ex)
        {
            return Result.Failure(ex.Code, ex.Message);
        }
    }
}