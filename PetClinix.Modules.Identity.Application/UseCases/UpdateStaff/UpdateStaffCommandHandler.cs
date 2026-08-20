using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Domain.Enums;
using PetClinix.Modules.Identity.Domain.Exceptions;
using PetClinix.Modules.Identity.Domain.Repositories;

namespace PetClinix.Modules.Identity.Application.UseCases.UpdateStaff;

public sealed class UpdateStaffCommandHandler : ICommandHandler<UpdateStaffCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStaffCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateStaffCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user == null || user.ClinicId != command.ClinicId)
        {
            return Result.Failure("identity.user.not_found", "Funcionário não encontrado nesta clínica.");
        }

        if (user.Role == UserRole.Admin)
        {
            return Result.Failure("identity.user.cannot_edit_admin", "Não é possível editar o perfil do Admin por esta rota.");
        }

        try
        {
            user.UpdateStaffInfo(command.Name, command.PhoneNumber, command.BirthDate, command.Role, command.UpdatedByUserId);

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (IdentityDomainException ex)
        {
            return Result.Failure(ex.Code, ex.Message);
        }
    }
}