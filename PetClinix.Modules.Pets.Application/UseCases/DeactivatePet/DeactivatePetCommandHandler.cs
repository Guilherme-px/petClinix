using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Pets.Application.Contracts;
using PetClinix.Modules.Pets.Domain.Repositories;

namespace PetClinix.Modules.Pets.Application.UseCases.DeactivatePet;

public sealed class DeactivatePetCommandHandler : ICommandHandler<DeactivatePetCommand, Result>
{
    private readonly IPetRepository _petRepository;
    private readonly IPetsUnitOfWork _unitOfWork;

    public DeactivatePetCommandHandler(IPetRepository petRepository, IPetsUnitOfWork unitOfWork)
    {
        _petRepository = petRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeactivatePetCommand command, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetByIdAsync(command.PetId, cancellationToken);

        if (pet == null || pet.ClinicId != command.ClinicId || pet.TutorId != command.TutorId)
        {
            return Result.Failure("pets.pet.not_found", "Pet não encontrado para este tutor.");
        }

        pet.Deactivate();

        await _petRepository.UpdateAsync(pet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}