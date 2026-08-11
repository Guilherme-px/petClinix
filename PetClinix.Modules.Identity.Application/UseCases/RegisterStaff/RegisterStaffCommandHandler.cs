using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Application.Contracts;
using PetClinix.Modules.Identity.Domain.Entities;
using PetClinix.Modules.Identity.Domain.Exceptions;
using PetClinix.Modules.Identity.Domain.Repositories;
using PetClinix.Modules.Identity.Domain.ValueObjects;

namespace PetClinix.Modules.Identity.Application.UseCases.RegisterStaff;

public sealed class RegisterStaffCommandHandler : ICommandHandler<RegisterStaffCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISubscriptionStatusService _subscriptionStatusService;
    private readonly IEmailService _emailService;

    public RegisterStaffCommandHandler(
       IUserRepository userRepository,
       IUnitOfWork unitOfWork,
       ISubscriptionStatusService subscriptionStatusService,
       IEmailService emailService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _subscriptionStatusService = subscriptionStatusService;
        _emailService = emailService;
    }

    public async Task<Result> Handle(RegisterStaffCommand command, CancellationToken cancellationToken)
    {
        var staffLimit = await _subscriptionStatusService.GetStaffLimitAsync(command.ClinicId, cancellationToken);
        var currentStaffCount = await _userRepository.CountByClinicIdAsync(command.ClinicId, cancellationToken);

        if (currentStaffCount >= staffLimit)
        {
            return Result.Failure("identity.staff_limit_reached", "O limite de funcionários do seu plano foi atingido. Faça um upgrade para adicionar mais.");
        }

        var emailVo = Email.Create(command.Email);

        if (await _userRepository.ExistsByEmailAsync(emailVo, cancellationToken))
        {
            return Result.Failure("identity.user.email_already_exists", "Já existe um usuário com esse e-mail.");
        }

        try
        {
            var staffUser = User.CreateStaff(
                command.ClinicId,
                command.Name,
                command.Email,
                null,
                command.DocumentNumber,
                command.PhoneNumber,
                command.BirthDate,
                command.Role);

            var token = staffUser.GeneratePasswordResetToken();

            await _userRepository.AddAsync(staffUser, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _emailService.SendWelcomeEmailAsync(staffUser.Email.Value, staffUser.Name, token, cancellationToken);

            return Result.Success();
        }
        catch (IdentityDomainException ex)
        {
            return Result.Failure(ex.Code, ex.Message);
        }
    }
}