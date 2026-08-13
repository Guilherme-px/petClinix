using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Pets.Domain.Entities;
using PetClinix.Modules.Pets.Domain.Exceptions;
using PetClinix.Modules.Pets.Domain.Repositories;
using PetClinix.Modules.Pets.Domain.ValueObjects;

namespace PetClinix.Modules.Pets.Application.UseCases.RegisterTutor;

public sealed class RegisterTutorCommandHandler : ICommandHandler<RegisterTutorCommand, Result>
{
    private readonly ITutorRepository _tutorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterTutorCommandHandler(ITutorRepository tutorRepository, IUnitOfWork unitOfWork)
    {
        _tutorRepository = tutorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RegisterTutorCommand command, CancellationToken cancellationToken)
    {
        var cpfVo = Cpf.Create(command.Cpf);

        if (await _tutorRepository.ExistsByCpfAsync(command.ClinicId, cpfVo, cancellationToken))
        {
            return Result.Failure("pets.tutor.cpf_already_exists", "Já existe um tutor cadastrado com este CPF.");
        }

        try
        {
            var tutor = Tutor.Create(
                command.ClinicId, command.Name, command.Cpf, command.Email, command.PhoneNumber, command.SecondaryPhoneNumber,
                command.ZipCode, command.Street, command.Number, command.Neighborhood, command.Complement, command.City, command.State, command.Notes
            );

            await _tutorRepository.AddAsync(tutor, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (PetsDomainException ex)
        {
            return Result.Failure(ex.Code, ex.Message);
        }
    }
}