using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Pets.Application.Contracts;
using PetClinix.Modules.Pets.Domain.Repositories;

namespace PetClinix.Modules.Pets.Application.UseCases.DeactivateTutor;

public sealed class DeactivateTutorCommandHandler : ICommandHandler<DeactivateTutorCommand, Result>
{
    private readonly ITutorRepository _tutorRepository;
    private readonly IPetsUnitOfWork _unitOfWork;

    public DeactivateTutorCommandHandler(ITutorRepository tutorRepository, IPetsUnitOfWork unitOfWork)
    {
        _tutorRepository = tutorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeactivateTutorCommand command, CancellationToken cancellationToken)
    {
        var tutor = await _tutorRepository.GetByIdAsync(command.TutorId, cancellationToken);

        if (tutor == null || tutor.ClinicId != command.ClinicId)
        {
            return Result.Failure("pets.tutor.not_found", "Tutor não encontrado nesta clínica.");
        }

        tutor.Deactivate();

        await _tutorRepository.UpdateAsync(tutor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}