using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Domain.Exceptions;
using PetClinix.Modules.Identity.Domain.Repositories;

namespace PetClinix.Modules.Identity.Application.UseCases.UpdateClinic;

public sealed class UpdateClinicCommandHandler : ICommandHandler<UpdateClinicCommand, Result>
{
    private readonly IClinicRepository _clinicRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateClinicCommandHandler(IClinicRepository clinicRepository, IUnitOfWork unitOfWork)
    {
        _clinicRepository = clinicRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateClinicCommand command, CancellationToken cancellationToken)
    {
        var clinic = await _clinicRepository.GetByIdAsync(command.ClinicId, cancellationToken);

        if (clinic == null)
        {
            return Result.Failure("identity.clinic.not_found", "Clínica não encontrada.");
        }

        try
        {
            clinic.UpdateInfo(
                command.TradeName, command.LegalName, command.DocumentNumber,
                command.Email, command.PhoneNumber,
                command.ZipCode, command.Street, command.Number, command.Neighborhood,
                command.Complement, command.City, command.State);

            await _clinicRepository.UpdateAsync(clinic, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (IdentityDomainException ex)
        {
            return Result.Failure(ex.Code, ex.Message);
        }
    }
}