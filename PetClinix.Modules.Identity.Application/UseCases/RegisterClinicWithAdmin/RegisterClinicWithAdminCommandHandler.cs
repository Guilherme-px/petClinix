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

    public RegisterClinicWithAdminCommandHandler(
        IClinicRepository clinicRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _clinicRepository = clinicRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<RegisterClinicWithAdminResponse>> Handle(
        RegisterClinicWithAdminCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var slug = ClinicSlug.Create(command.Slug);
            var clinicEmail = Email.Create(command.Email);
            var adminEmail = Email.Create(command.AdminEmail);

            if (await _clinicRepository.ExistsBySlugAsync(slug, cancellationToken))
            {
                return Result<RegisterClinicWithAdminResponse>.Failure(
                    "identity.clinic.slug_already_exists",
                    "Já existe uma clínica com esse identificador.");
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
                command.Slug,
                command.Email,
                command.PhoneNumber,
                command.AddressLine,
                command.City,
                command.State,
                command.ZipCode);

            var passwordHash = _passwordHasher.Hash(command.Password);

            var adminUser = User.CreateAdmin(
                clinic.Id,
                command.AdminName,
                command.AdminEmail,
                passwordHash);

            await _clinicRepository.AddAsync(clinic, cancellationToken);
            await _userRepository.AddAsync(adminUser, cancellationToken);

            return Result<RegisterClinicWithAdminResponse>.Success(
                new RegisterClinicWithAdminResponse
                {
                    ClinicId = clinic.Id,
                    AdminUserId = adminUser.Id
                });
        }
        catch (IdentityDomainException ex)
        {
            return Result<RegisterClinicWithAdminResponse>.Failure(ex.Code, ex.Message);
        }
    }
}