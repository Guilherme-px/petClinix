using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Application.Contracts;
using PetClinix.Modules.Identity.Domain.Entities;
using PetClinix.Modules.Identity.Domain.Exceptions;
using PetClinix.Modules.Identity.Domain.Repositories;
using PetClinix.Modules.Identity.Domain.ValueObjects;

namespace PetClinix.Modules.Identity.Application.UseCases.RegisterClinicWithAdmin;

public sealed class RegisterClinicWithAdminCommandHandler
    : ICommandHandler<RegisterClinicWithAdminCommand, Result<RegisterClinicWithAdminResponse>>
{
    private readonly IClinicRepository _clinicRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public RegisterClinicWithAdminCommandHandler(
        IClinicRepository clinicRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IEmailService emailService)
    {
        _clinicRepository = clinicRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<Result<RegisterClinicWithAdminResponse>> Handle(
        RegisterClinicWithAdminCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var slug = ClinicSlug.CreateFromName(command.TradeName);
            var clinicEmail = Email.Create(command.Email);
            var adminEmail = Email.Create(command.AdminEmail);

            if (await _clinicRepository.ExistsBySlugAsync(slug, cancellationToken))
            {
                return Result<RegisterClinicWithAdminResponse>.Failure(
                    "identity.clinic.slug_already_exists",
                    "Já existe uma clínica com esse nome/identificador.");
            }

            if (await _clinicRepository.ExistsByEmailAsync(clinicEmail, cancellationToken))
            {
                return Result<RegisterClinicWithAdminResponse>.Failure(
                    "identity.clinic.email_already_exists",
                    "Já existe uma clínica com esse e-mail.");
            }

            if (await _userRepository.ExistsByEmailAsync(adminEmail, cancellationToken))
            {
                return Result<RegisterClinicWithAdminResponse>.Failure(
                    "identity.user.email_already_exists",
                    "Já existe um usuário com esse e-mail.");
            }

            var clinic = Clinic.Create(
                command.TradeName,
                command.LegalName,
                command.DocumentNumber,
                slug,
                command.Email,
                command.PhoneNumber,
                command.ZipCode,
                command.Street,
                command.Number,
                command.Neighborhood,
                command.Complement,
                command.City,
                command.State);

            var adminUser = User.CreateAdmin(
                clinic.Id,
                Guid.Empty,
                command.AdminName,
                command.AdminEmail,
                null,
                command.AdminDocumentNumber,
                command.AdminPhoneNumber,
                command.AdminBirthDate);

            var token = adminUser.GeneratePasswordResetToken();

            await _clinicRepository.AddAsync(clinic, cancellationToken);
            await _userRepository.AddAsync(adminUser, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var returnedToken = await _emailService.SendWelcomeEmailAsync(adminUser.Email.Value, adminUser.Name, token, cancellationToken);

            return Result<RegisterClinicWithAdminResponse>.Success(
                new RegisterClinicWithAdminResponse
                {
                    ClinicId = clinic.Id,
                    AdminUserId = adminUser.Id,
                    PasswordResetToken = returnedToken
                });
        }
        catch (IdentityDomainException ex)
        {
            return Result<RegisterClinicWithAdminResponse>.Failure(ex.Code, ex.Message);
        }
    }
}